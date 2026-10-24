// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Omu.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;
using Robust.Shared.Timing;

namespace Content.Omu.Client.Weapons.Ranged;

public sealed class ChargedShotReticleOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly SharedGunSystem _gun;
    private readonly SpriteSystem _sprite;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public ChargedShotReticleOverlay()
    {
        IoCManager.InjectDependencies(this);

        _gun = _entity.System<SharedGunSystem>();
        _sprite = _entity.System<SpriteSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is not { } user
            || !_gun.TryGetGun(user, out var gun)
            || !_entity.TryGetComponent(gun, out ChargedShotComponent? charged)
            || charged.Level == 0
            || charged.Levels[charged.Level - 1].Reticle is not { } reticle)
            return;

        var mouse = _input.MouseScreenPosition;
        if (_eye.PixelToMap(mouse).MapId != args.MapId)
            return;

        var texture = _sprite.GetFrame(reticle, _timing.CurTime);
        var size = texture.Size * ((args.ViewportControl as Control)?.UIScale ?? 1f);
        args.ScreenHandle.DrawTextureRect(texture, UIBox2.FromDimensions(mouse.Position - size * 0.5f, size));
    }
}
