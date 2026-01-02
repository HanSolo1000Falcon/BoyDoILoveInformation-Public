using System.Linq;
using BoyDoILoveInformation.Core;
using BoyDoILoveInformation.Patches;
using TMPro;

namespace BoyDoILoveInformation.Tab_Handlers;

public class ActionsHandler : TabHandlerBase
{
    private void Start()
    {
        transform.GetChild(0).AddComponent<BDILIButton>().OnPress = () =>
                                                                    {
                                                                        GorillaPlayerScoreboardLine scoreboardLine =
                                                                                GorillaScoreboardTotalUpdater
                                                                                       .allScoreboardLines
                                                                                       .FirstOrDefault(line =>
                                                                                                    line.linePlayer ==
                                                                                                    InformationHandler
                                                                                                           .ChosenRig
                                                                                                           .OwningNetPlayer);

                                                                        if (scoreboardLine == null)
                                                                            return;

                                                                        scoreboardLine.PressButton(true,
                                                                                GorillaPlayerLineButton.ButtonType
                                                                                       .Cheating);
                                                                    };

        transform.GetChild(1).AddComponent<BDILIButton>().OnPress = () =>
                                                                    {
                                                                        GorillaPlayerScoreboardLine scoreboardLine =
                                                                                GorillaScoreboardTotalUpdater
                                                                                       .allScoreboardLines
                                                                                       .FirstOrDefault(line =>
                                                                                                    line.linePlayer ==
                                                                                                    InformationHandler
                                                                                                           .ChosenRig
                                                                                                           .OwningNetPlayer);

                                                                        if (scoreboardLine == null)
                                                                            return;

                                                                        scoreboardLine.PressButton(true,
                                                                                GorillaPlayerLineButton.ButtonType
                                                                                       .Toxicity);
                                                                    };

        transform.GetChild(2).AddComponent<BDILIButton>().OnPress = () =>
                                                                    {
                                                                        GorillaPlayerScoreboardLine scoreboardLine =
                                                                                GorillaScoreboardTotalUpdater
                                                                                       .allScoreboardLines
                                                                                       .FirstOrDefault(line =>
                                                                                                    line.linePlayer ==
                                                                                                    InformationHandler
                                                                                                           .ChosenRig
                                                                                                           .OwningNetPlayer);

                                                                        if (scoreboardLine == null)
                                                                            return;

                                                                        scoreboardLine.PressButton(true,
                                                                                GorillaPlayerLineButton.ButtonType
                                                                                       .HateSpeech);
                                                                    };

        transform.GetChild(3).AddComponent<BDILIButton>().OnPress = () =>
                                                                    {
                                                                        GorillaPlayerScoreboardLine scoreboardLine =
                                                                                GorillaScoreboardTotalUpdater
                                                                                       .allScoreboardLines
                                                                                       .FirstOrDefault(line =>
                                                                                                    line.linePlayer ==
                                                                                                    InformationHandler
                                                                                                           .ChosenRig
                                                                                                           .OwningNetPlayer);

                                                                        if (scoreboardLine == null)
                                                                            return;

                                                                        scoreboardLine.muteButton.isOn =
                                                                                !scoreboardLine.muteButton.isOn;

                                                                        scoreboardLine.PressButton(
                                                                                scoreboardLine.muteButton.isOn,
                                                                                GorillaPlayerLineButton.ButtonType
                                                                                       .Mute);

                                                                        transform.GetChild(3)
                                                                               .GetComponentInChildren<TextMeshPro>()
                                                                               .text = scoreboardLine.muteButton.isOn
                                                                                    ? "Unmute"
                                                                                    : "Mute";
                                                                    };

        transform.GetChild(4).AddComponent<BDILIButton>().OnPress = () =>
                                                                    {
                                                                        if (VoicePrioritizationPatch.PrioritizedPeople
                                                                           .Contains(InformationHandler
                                                                                   .ChosenRig))
                                                                            VoicePrioritizationPatch.PrioritizedPeople
                                                                                   .Remove(InformationHandler
                                                                                           .ChosenRig);
                                                                        else
                                                                            VoicePrioritizationPatch.PrioritizedPeople
                                                                                   .Add(InformationHandler
                                                                                           .ChosenRig);

                                                                        transform.GetChild(4)
                                                                               .GetComponentInChildren<TextMeshPro>()
                                                                               .text = VoicePrioritizationPatch
                                                                                   .PrioritizedPeople.Contains(
                                                                                            InformationHandler
                                                                                                   .ChosenRig)
                                                                                    ? "Unprioritize"
                                                                                    : "Prioritize";
                                                                    };

        transform.GetChild(5).AddComponent<BDILIButton>().OnPress =
                () => VoicePrioritizationPatch.PrioritizedPeople.Clear();
    }
}