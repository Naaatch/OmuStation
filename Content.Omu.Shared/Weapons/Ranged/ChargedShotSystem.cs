// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Omu.Shared.Weapons.Ranged;

public sealed class ChargedShotSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChargedShotComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<ChargedShotComponent, HandDeselectedEvent>(OnHandDeselected);
        SubscribeLocalEvent<ChargedShotComponent, GotUnequippedHandEvent>(OnUnequippedHand);
        SubscribeLocalEvent<CombatModeToggledEvent>(OnCombatModeToggled);
        SubscribeAllEvent<RequestStopShootEvent>(OnStopShootRequest);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ChargedShotComponent>();
        while (query.MoveNext(out var uid, out var charged))
        {
            var level = GetLevel(charged);
            if (level == charged.Level)
                continue;

            if (level > charged.Level && _net.IsServer)
                _audio.PlayPvs(charged.Levels[level - 1].Sound, uid);

            charged.Level = level;
            _appearance.SetData(uid, ChargedShotVisuals.Level, level);
        }
    }

    private void OnShotAttempted(Entity<ChargedShotComponent> ent, ref ShotAttemptedEvent args)
    {
        if (ent.Comp.Releasing)
            return;

        ent.Comp.Aim = args.Used.Comp.ShootCoordinates;
        ent.Comp.Target = args.Used.Comp.Target;
        args.Cancel();

        if (ent.Comp.ChargeStart != null)
            return;

        ent.Comp.ChargeStart = _timing.CurTime;
        Dirty(ent);

        args.Used.Comp.ShotCounter = 1;
        DirtyField(args.Used.AsNullable(), nameof(GunComponent.ShotCounter));

        _audio.PlayPredicted(ent.Comp.ChargeSound, ent, args.User);
    }

    private void OnHandDeselected(Entity<ChargedShotComponent> ent, ref HandDeselectedEvent args)
    {
        ResetCharge(ent);
    }

    private void OnUnequippedHand(Entity<ChargedShotComponent> ent, ref GotUnequippedHandEvent args)
    {
        ResetCharge(ent);
    }

    private void OnCombatModeToggled(ref CombatModeToggledEvent args)
    {
        if (_gun.TryGetGun(args.User, out var gun) && TryComp<ChargedShotComponent>(gun, out var charged))
            ResetCharge((gun, charged));
    }

    private void OnStopShootRequest(RequestStopShootEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user
            || !_gun.TryGetGun(user, out var gun)
            || gun.Owner != GetEntity(msg.Gun)
            || !TryComp<ChargedShotComponent>(gun, out var charged)
            || charged.ChargeStart == null
            || charged.Aim is not { } aim
            || !TryComp<BasicEntityAmmoProviderComponent>(gun, out var ammo))
            return;

        var projectile = ammo.Proto;
        var level = GetLevel(charged);
        if (level > 0)
            ammo.Proto = charged.Levels[level - 1].Projectile;

        charged.Releasing = true;
        gun.Comp.ShotCounter = 0;
        _gun.AttemptShoot(user, gun, aim, charged.Target);
        charged.Releasing = false;
        ammo.Proto = projectile;

        ResetCharge((gun, charged));
    }

    private int GetLevel(ChargedShotComponent charged)
    {
        if (charged.ChargeStart is not { } start)
            return 0;

        var held = _timing.CurTime - start;
        return charged.Levels.Count(level => held >= level.Delay);
    }

    private void ResetCharge(Entity<ChargedShotComponent> ent)
    {
        if (ent.Comp.ChargeStart == null)
            return;

        ent.Comp.ChargeStart = null;
        Dirty(ent);
    }
}
