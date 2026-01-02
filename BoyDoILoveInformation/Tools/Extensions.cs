using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;

namespace BoyDoILoveInformation.Tools;

public enum GamePlatform
{
    Steam,
    OculusPC,
    PC,
    Standalone,
    Unknown,
}

public static class Extensions
{
    public static          Dictionary<VRRig, GamePlatform> PlayerPlatforms      = new();
    public static          Dictionary<VRRig, List<string>> PlayerMods           = new();
    public static          Dictionary<string, DateTime>    AccountCreationDates = new();
    public static          List<VRRig>                     PlayersWithCosmetics = [];
    public static readonly Dictionary<VRRig, int>          PlayerPing           = new();

    public static GamePlatform GetPlatform(this VRRig rig) =>
            PlayerPlatforms.GetValueOrDefault(rig, GamePlatform.Unknown);

    public static string ParsePlatform(this GamePlatform gamePlatform)
    {
        return gamePlatform switch
               {
                       GamePlatform.Unknown    => "<color=#000000>Unknown</color>",
                       GamePlatform.Steam      => "<color=#0091F7>Steam</color>",
                       GamePlatform.OculusPC   => "<color=#0091F7>Oculus PCVR</color>",
                       GamePlatform.PC         => "<color=#000000>PC</color>",
                       GamePlatform.Standalone => "<color=#26A6FF>Standalone</color>",
                       var _                   => throw new ArgumentOutOfRangeException(),
               };
    }

    public static DateTime GetAccountCreationDate(this VRRig rig) => AccountCreationDates[rig.OwningNetPlayer.UserId];

    public static string[] GetPlayerMods(this VRRig rig) => PlayerMods[rig].ToArray();

    public static bool HasCosmetics(this VRRig rig) => PlayersWithCosmetics.Contains(rig);

    public static int GetPing(this VRRig rig) =>
            PlayerPing.TryGetValue(rig, out int ping) ? ping : PhotonNetwork.GetPing();

    public static string InsertNewlinesWithRichText(this string input, int interval)
    {
        if (string.IsNullOrEmpty(input) || interval <= 0)
            return input;

        StringBuilder output                       = new();
        int           visibleCount                 = 0;
        int           lastWhitespaceIndex          = -1;
        int           outputLengthAtLastWhitespace = -1;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '<')
            {
                int tagEnd = input.IndexOf('>', i);
                if (tagEnd == -1)
                {
                    output.Append(c);

                    continue;
                }

                output.Append(input.AsSpan(i, tagEnd - i + 1));
                i = tagEnd;

                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                lastWhitespaceIndex          = i;
                outputLengthAtLastWhitespace = output.Length;
            }

            output.Append(c);
            visibleCount++;

            if (visibleCount >= interval)
            {
                if (outputLengthAtLastWhitespace != -1)
                {
                    output[outputLengthAtLastWhitespace] = '\n';
                    visibleCount                         = i - lastWhitespaceIndex;
                    lastWhitespaceIndex                  = -1;
                    outputLengthAtLastWhitespace         = -1;
                }
                else
                {
                    output.Append('\n');
                    visibleCount = 0;
                }
            }
        }

        return output.ToString();
    }
}