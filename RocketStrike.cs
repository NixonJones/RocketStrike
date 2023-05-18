using System.Collections.Generic;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Admin Rocket Strike", "Tesla", "1.0.1")]
    [Description("Allows admins to shoot rockets from different positions around their character.")]
    class RocketStrike : RustPlugin
    {
        private Dictionary<ulong, float> lastCommandTimes = new Dictionary<ulong, float>();
        private float commandCooldown = 3f; // Adjust the cooldown duration as needed

        private Vector3[] rocketSpawnOffsets = new Vector3[]
        {
            // Spawn areas
            new Vector3(0f, 0f, 2f),
            new Vector3(2f, 0f, 0f),
            new Vector3(2f, 0f, -2f),
            new Vector3(-2f, 2f, -4f),
            new Vector3(-2f, -3f, 0f),
            new Vector3(-2f, -2f, 1f),
            new Vector3(-2f, -6f, 0f),
            new Vector3(-6f, -8f, 0f),
            new Vector3(-4f, 0f, 4f),
            new Vector3(-2f, 0f, 2f),
            new Vector3(3f, 6f, 2f),
            new Vector3(4f, 0f, 0f),
            new Vector3(6f, 8f, 4f),
            new Vector3(8f, 1f, 2f),
            new Vector3(10f, 5f, 12f),
            new Vector3(-2f, 3f, 8f),
            new Vector3(2f, 6f, 4f),
            new Vector3(-10f, 0f, 0f),
            new Vector3(-2f, 5f, -8f),
            new Vector3(-12f, 10f, -12f)
        };

        [ChatCommand("strike")]
        private void CmdRocketStrike(BasePlayer player, string command, string[] args)
        {
            if (!IsAdmin(player))
                return;

            if (args.Length < 1)
            {
                SendChatMessage(player, "Syntax: /strike [amount]");
                return;
            }

            int amount;
            if (!int.TryParse(args[0], out amount))
            {
                SendChatMessage(player, "Invalid amount specified.");
                return;
            }

            if (amount <= 0)
            {
                SendChatMessage(player, "Amount must be greater than zero.");
                return;
            }

            if (amount > 20)
            {
                SendChatMessage(player, "<color=#aa4735>Stuff does not allow you to do more than 20 rockets... What were you thinking?</color>");
                return;
            }

            if (IsSpamming(player))
            {
                SendChatMessage(player, "<color=#aa4735>You're spamming.</color>");
                return;
            }

            ShootRockets(player, amount);
            lastCommandTimes[player.userID] = Time.realtimeSinceStartup;
        }

        [ConsoleCommand("strike")]
        private void ConsoleRocketStrike(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin)
                return;

            if (arg.Args.Length < 1)
            {
                SendConsoleMessage(arg, "Syntax: strike <amount>");
                return;
            }

            int amount;
            if (!int.TryParse(arg.Args[0], out amount))
            {
                SendConsoleMessage(arg, "Invalid amount specified.");
                return;
            }

            if (amount <= 0)
            {
                SendConsoleMessage(arg, "Amount must be greater than zero.");
                return;
            }

            if (amount > 20)
            {
                SendConsoleMessage(arg, "Tesla does not allow you to do more than 20 rockets... What were you thinking?");
                return;
            }

            BasePlayer player = arg.Player();
            if (player != null)
            {
                if (IsSpamming(player))
                {
                    SendChatMessage(player, "You're spamming.");
                    return;
                }

                ShootRockets(player, amount);
                lastCommandTimes[player.userID] = Time.realtimeSinceStartup;
            }
        }

        private bool IsSpamming(BasePlayer player)
        {
            if (!lastCommandTimes.ContainsKey(player.userID))
                return false;

            float lastCommandTime = lastCommandTimes[player.userID];
            float currentTime = Time.realtimeSinceStartup;
            return (currentTime - lastCommandTime) < commandCooldown;
        }

        private void ShootRockets(BasePlayer player, int amount)
        {
            if (player == null || player.IsDead())
                return;

            for (int i = 0; i < amount; i++)
            {
                int spawnIndex = i % rocketSpawnOffsets.Length;
                Vector3 rocketSpawnPosition = player.transform.position + player.transform.TransformDirection(rocketSpawnOffsets[spawnIndex]);

                BaseEntity rocket = GameManager.server.CreateEntity("assets/prefabs/ammo/rocket/rocket_fire.prefab", rocketSpawnPosition, Quaternion.identity);
                if (rocket != null)
                {
                    rocket.Spawn();
                    rocket.GetComponent<ServerProjectile>()?.InitializeVelocity(player.eyes.BodyForward() * 50f); // Adjust the rocket speed as needed

                    // Delay the explosion using a timer
                    timer.Once(10f, () =>
                    {
                        if (rocket.IsValid() && !rocket.IsDestroyed)
                        {
                            rocket.GetComponent<TimedExplosive>()?.Explode();
                        }
                    });
                }
            }
        }

        private bool IsAdmin(BasePlayer player)
        {
            return player.IsAdmin;
        }

        private void SendChatMessage(BasePlayer player, string message)
        {
            if (player != null)
                player.ChatMessage(message);
        }

        private void SendConsoleMessage(ConsoleSystem.Arg arg, string message)
        {
            if (arg != null && arg.Connection != null)
                SendChatMessage(arg.Player(), message);
        }
    }
}
