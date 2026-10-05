// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Omu.Shared.Entities.Voodoo;

[Serializable, NetSerializable]
public enum VoodooDollUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public readonly record struct VoodooDollCandidate(NetEntity Entity, string Name, EntProtoId? Prototype);

[Serializable, NetSerializable]
public sealed class VoodooDollBoundUserInterfaceState(List<VoodooDollCandidate> candidates) : BoundUserInterfaceState
{
    public readonly List<VoodooDollCandidate> Candidates = candidates;
}

[Serializable, NetSerializable]
public sealed class VoodooDollChooseVictimMessage(NetEntity victim) : BoundUserInterfaceMessage
{
    public readonly NetEntity Victim = victim;
}
