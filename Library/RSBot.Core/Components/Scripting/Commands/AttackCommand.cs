using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;
using RSBot.Core.Plugins;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RSBot.Core.Components.Scripting.Commands;

/// <summary>
/// Skeleton for a future "attackhere" script command.
/// Add targeting/movement logic where marked once the behaviour is defined.
/// </summary>
internal class AttackCommand : IScriptCommand
{
    /// <summary>
    /// Token used in script files. Example line: "attackhere 123.4 456.7 3.0 20".
    /// Adjust to upper/lowercase usage if scripts should differ.
    /// </summary>
    public string Name => "attack";

    public bool IsBusy { get; private set; }

    /// <summary>
    /// Arguments are placeholders; adapt once the final behaviour is known.
    /// Example idea: "XOffset", "YOffset", "ZOffset", "Radius" for AoE or pull range.
    /// </summary>
    public Dictionary<string, string> Arguments => new()
    {
        { "Radius", "The Radius for this Area" }
    };

    /// <summary>
    /// Entry point called by ScriptManager. Insert movement + combat logic here:
    /// - Parse coords/radius, move to spot (similar to MoveScriptCommand) if needed
    /// - Acquire targets in radius, validate line of sight
    /// - Trigger combat routines or specific skill rotation
    /// Currently returns false to signal unimplemented.
    /// </summary>
    public bool Execute(string[] arguments = null)
    {
        if (arguments == null || arguments.Length == 0)
        {
            Log.Warn("[Script] Invalid attack command: Radius information missing.");

            return false;
        }
        if (IsBusy)
            return false;
        try
        {
            IsBusy = true;

            if (!Game.Player.CanAttack)
            {
                Log.Warn("[Script] Attack command blocked: player cannot attack right now.");
                return false;
            }

            if (!int.TryParse(arguments[0], out var radius))
                return false;

            var position = Game.Player.Position;

            if (!AnyMonsterInRange(position, radius))
            {
                Log.Notify($"[Script] No monsters within {radius:0.#}m. Skipping attack command.");
                return true;
            }

            Log.Notify($"[Script] Attacking monsters within {radius:0.#}m ...");

            while (ScriptManager.Running && !ScriptManager.Paused)
            {
                var currentPostion = Game.Player.Position;
                var target = GetNextTarget(currentPostion, radius);
                if (target == null)
                {
                    bool moveResult = Game.Player.MoveTo(position);
                    Thread.Sleep(1000);
                    target = GetNextTarget(currentPostion, radius);
                    if (target == null)
                        break;
                    AttackTarget(target, position, radius);
                }
                    
                AttackTarget(target,position,radius);

            }

            Log.Notify("[Script] Area cleared, continuing script.");
            return true;

        }
        finally
        {
            IsBusy = false;
        }
    }
    private static bool AnyMonsterInRange(Position position, double radius)
    {
        return SpawnManager.Any<SpawnedMonster>(m => IsTargetable(m, position, radius));
    }
    private static SpawnedMonster GetNextTarget(Position position, double radius)
    {
        if (!SpawnManager.TryGetEntities<SpawnedMonster>(m => IsTargetable(m, position, radius), out var monsters))
            return null;

        return monsters
            .OrderBy(m => m.Movement.Source.DistanceTo(position))
            .FirstOrDefault();
    }
    private static bool IsTargetable(SpawnedMonster monster, Position anchor, double radius)
    {
        if (monster == null || !monster.HasHealth || monster.State.LifeState == LifeState.Dead)
            return false;

        // keep the fight contained to the original area
        return monster.Movement.Source.DistanceTo(anchor) <= radius;
    }
    private static void AttackTarget(SpawnedMonster target, Position position, double radius)
    {
        // Ensure we are close enough; if not, move back toward the anchor before attacking
        if (target.Movement.Source.DistanceTo(position) > radius)
            return;

        var retry = 0;
        while (!target.TrySelect() && retry++ < 3)
            Thread.Sleep(200);

        if (Game.SelectedEntity?.UniqueId != target.UniqueId)
            Game.SelectedEntity = target;

        var elapsed = 0;
        const int timeout = 12000; // safety to avoid infinite loops

        while (target.HasHealth && target.State.LifeState != LifeState.Dead && elapsed < timeout)
        {
            if (!ScriptManager.Running || ScriptManager.Paused)
                return;

            if (!Game.Player.CanAttack)
            {
                Thread.Sleep(200);
                elapsed += 200;
                continue;
            }

            // Use configured skills if available, otherwise fall back to auto-attack
            var skill = SkillManager.GetNextSkill();
            var casted = skill != null
                ? SkillManager.CastSkill(skill, target.UniqueId)
                : SkillManager.CastAutoAttack();

            if (!casted)
            {
                Thread.Sleep(300);
                elapsed += 300;
                continue;
            }

            Thread.Sleep(300);
            elapsed += 300;
        }
    }
    public void Stop()
    {
        // TODO: Cancel any pending movement/attack tasks; reset state.
        IsBusy = false;
    }
}
