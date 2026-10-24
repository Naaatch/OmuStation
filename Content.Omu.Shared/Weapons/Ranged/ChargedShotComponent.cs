// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Omu.Shared.Weapons.Ranged;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ChargedShotComponent : Component
{
    [DataField(required: true)]
    public List<ChargedShotLevel> Levels = new();

    [DataField]
    public SoundSpecifier? ChargeSound;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan? ChargeStart;

    [ViewVariables]
    public int Level;

    [ViewVariables]
    public bool Releasing;

    [ViewVariables]
    public EntityCoordinates? Aim;

    [ViewVariables]
    public EntityUid? Target;
}

[DataDefinition]
public sealed partial class ChargedShotLevel
{
    [DataField(required: true)]
    public TimeSpan Delay;

    [DataField(required: true)]
    public EntProtoId Projectile;

    [DataField]
    public SpriteSpecifier? Reticle;

    [DataField]
    public SoundSpecifier? Sound;
}

[Serializable, NetSerializable]
public enum ChargedShotVisuals : byte
{
    Level,
}
