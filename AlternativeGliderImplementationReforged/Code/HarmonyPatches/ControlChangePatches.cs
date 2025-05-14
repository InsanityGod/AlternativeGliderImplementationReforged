using AlternativeGliderImplementationReforged.Config;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace AlternativeGliderImplementationReforged.Code.HarmonyPatches
{
    [HarmonyPatch]
    public static class ControlChangePatches
    {
        //TODO maybe have wind affect stuff
        //TODO maybe move this to config
        public const double speedMin = 0.0005;

        public const double speedMid = 0.0025;
        public const double speedMax = 0.01;

        public const double diveFallLimit = -0.10;
        public const double diveFallAcc = -0.0025;
        public const double diveFallMult = 1.25;

        public const double glideFallLimit = -0.05;
        public const double glideFallResist = 0.01;
        public const double glideSpeedAcc = 0.0001;
        public const double glideSpeedDec = 0.000005;

        public const double brakeSpeedDec = 0.00025;
        public const double brakeMotionPctDec = 1.0;
        public const double brakeLiftAcc = 0.035;
        public const double brakeHmotionLimit = 0.02;
        public const double brakeFallLimit = -0.025;
        public const double brakeFallResist = 0.01;

        public const float brakePitch = (float)(-30 * (Math.PI / 180));

        public const float pitchGrace = (float)(1 * (Math.PI / 180));

        [HarmonyPatch(typeof(PModulePlayerInAir), nameof(PModulePlayerInAir.ApplyFlying))]
        [HarmonyPrefix]
        public static bool ChangeNormalControls(EntityPos pos, EntityControls controls)
        {
            if (!controls.Gliding) return true;

            double cosYaw = Math.Cos(Math.PI - pos.Yaw);
            double sinYaw = Math.Sin(Math.PI - pos.Yaw);

            bool diving = AltGliderServerConfig.Instance.AlternativeDiveControls ?
                controls.Sneak :
                controls.Forward;

            bool braking = AltGliderServerConfig.Instance.AlternativeBrakeControls ?
                controls.Jump :
                controls.Backward;

            // Vertical.
            if (diving)
            {
                // Dive.
                double power = -(pos.Motion.Y / 60) * diveFallMult;
                if (power > controls.GlideSpeed)
                {
                    // Transfer fall speed to glide speed.
                    controls.GlideSpeed = Math.Max(power, 0);
                }

                if (pos.Motion.Y > diveFallLimit)
                {
                    // Accelerate to terminal velocity.
                    pos.Motion.Y = Math.Max(pos.Motion.Y + diveFallAcc, diveFallLimit);
                }
            }
            else if (braking && controls.GlideSpeed > speedMin)
            {
                // Brake.
                double hmotion = Math.Sqrt(Math.Pow(pos.Motion.X / 60, 2) + Math.Pow(pos.Motion.Z / 60, 2));
                double dec = Math.Min(controls.GlideSpeed - brakeSpeedDec, brakeSpeedDec);
                controls.GlideSpeed = Math.Max(controls.GlideSpeed - dec, speedMin);

                if (hmotion > speedMid)
                {
                    // Partial lift for partial speed decrease.
                    double lift = (dec / brakeSpeedDec) * brakeLiftAcc;

                    // Lift proportionally to horizontal motion.
                    pos.Motion.Y += lift * Math.Min(hmotion / brakeHmotionLimit, 1);
                }
                else
                {
                    // Reduce fall limit proportionally to speed.
                    double fallLimit = brakeFallLimit * (1 - (controls.GlideSpeed / speedMax));

                    if (pos.Motion.Y < fallLimit)
                    {
                        // Break fall until glide terminal velocity.
                        pos.Motion.Y = Math.Min(pos.Motion.Y + brakeFallResist, fallLimit);
                    }
                }

                // Further decrease horizontal motion.
                double motionDec = hmotion * -brakeMotionPctDec;
                pos.Motion.Add(sinYaw * motionDec,
                        0,
                        cosYaw * -motionDec);
            }
            else
            {
                // Glide.
                if (controls.GlideSpeed < speedMid)
                {
                    // Accelerate to minimum speed.
                    controls.GlideSpeed = Math.Min(controls.GlideSpeed + glideSpeedAcc, speedMid);
                }
                else
                {
                    // Decelerate to minimum speed.
                    controls.GlideSpeed = Math.Max(controls.GlideSpeed - glideSpeedDec, speedMid);
                }

                // Reduce fall limit proportionally to speed.
                double fallLimit = glideFallLimit * (1 - (controls.GlideSpeed / speedMax));

                if (pos.Motion.Y < fallLimit)
                {
                    // Break fall until glide terminal velocity.
                    pos.Motion.Y = Math.Min(pos.Motion.Y + glideFallResist, fallLimit);
                }
            }

            // Horizontal.
            pos.Motion.X += sinYaw * controls.GlideSpeed;
            pos.Motion.Z += cosYaw * -controls.GlideSpeed;

            // Punish sharp turns.
            double speed = Math.Sqrt(Math.Pow(pos.Motion.X / 60, 2) + Math.Pow(pos.Motion.Y / 60, 2) + Math.Pow(pos.Motion.Z / 60, 2));
            controls.GlideSpeed = Math.Min(controls.GlideSpeed, speed);

            // Skip original method.
            return false;
        }

        // Match pitch to glide direction.
        [HarmonyPatch(typeof(EntityBehaviorPlayerPhysics), nameof(EntityBehaviorPlayerPhysics.SetPlayerControls))]
        [HarmonyPostfix]
        public static void PatchRotation(EntityBehaviorPlayerPhysics __instance)
        {
            EntityPlayer PlayerEntity = __instance.entity as EntityPlayer;

            IPlayer player = PlayerEntity.Player;
            EntityControls controls = PlayerEntity.Controls;

            // Copy original checks.
            if (PlayerEntity.World.Side == EnumAppSide.Server && ((IServerPlayer)player).ConnectionState != EnumClientState.Playing)
            {
                return;
            }

            EntityPos pos = PlayerEntity.World.Side == EnumAppSide.Server
                    ? PlayerEntity.ServerPos
                    : PlayerEntity.Pos;

            if (controls.Gliding)
            {
                PlayerEntity.WalkPitch = (float)-pos.Motion.Y; //TODO I don't quite understand the purpose of this in the original code so I left it in for now
            }
            else if (!PlayerEntity.Swimming)
            {
                // Fix pitch bug.
                PlayerEntity.WalkPitch = 0;
            }
        }

        public static bool ClimbingCheck(EntityControls controls) => AltGliderServerConfig.Instance.AllowGlideWhileClimbing || !controls.IsClimbing;

        [HarmonyPatch(typeof(ModSystemGliding), "Input_InWorldAction")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AppendClimbingCheck(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var label = generator.DefineLabel();
            var fieldToFind = AccessTools.Field(typeof(EntityControls), nameof(EntityControls.IsFlying));
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Ldfld && code.operand is FieldInfo field && field == fieldToFind && codes[i + 1].opcode == OpCodes.Brtrue_S)
                {
                    var labelToFind = (Label)codes[i + 1].operand;
                    codes.Find(code => code.labels.Contains(labelToFind)).labels.Add(label);

                    codes.InsertRange(i + 2, new CodeInstruction[]
                    {
                        new(OpCodes.Ldloc_0), //Load this
                        new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(EntityAgent), nameof(EntityAgent.Controls))), //Get controls
                        new(OpCodes.Call, AccessTools.Method(typeof(ControlChangePatches), nameof(ClimbingCheck))), //check if we fullfill AllowGlideWhileClimbing
                        new(OpCodes.Brfalse_S, label)
                    });

                    break;
                }
            }

            return codes;
        }
    }
}