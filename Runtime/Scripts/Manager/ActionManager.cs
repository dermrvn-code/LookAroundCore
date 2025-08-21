using System.Text.RegularExpressions;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    SceneManagerBase sceneManager;
    SceneChangerBase sceneChanger;
    PuzzleManager puzzleManager;

    void Start()
    {
        sceneManager = FindFirstObjectByType<SceneManagerBase>();
        sceneChanger = FindFirstObjectByType<SceneChangerBase>();
        puzzleManager = FindFirstObjectByType<PuzzleManager>();
    }



    public void ActionParser(string action)
    {
        Debug.Log(action);
        string pattern;
        Match match;

        if (string.IsNullOrEmpty(action)) return;

        // TO SCENE
        pattern = @"\btoScene\(([^,)]+?)(?:,(-?\d))*\)";
        match = Regex.Match(action, pattern);
        if (match.Success)
        {
            ParseToScene(match);
            return;
        }

        // START MINIGAME
        pattern = @"\bstartMiniGame\(([^,)]+?)(?:,([^,)]+?))?(?:,(-?\d))?\)";
        match = Regex.Match(action, pattern);
        if (match.Success)
        {
            ParseStartMiniGame(match);
            return;
        }

        // RESTART MINIGAME
        pattern = @"\brestartMiniGame\(([^,)]+?)(?:,([^,)]+?))?(?:,(-?\d))?\)";
        match = Regex.Match(action, pattern);
        if (match.Success)
        {
            ParseRestartMiniGame(match);
            return;
        }

        Debug.Log($"Could not parse '{action}'");


    }

    void GoToScene(string sceneName, int animationIndex)
    {
        if (sceneName == "") return;
        if (sceneManager.sceneList.TryGetValue(sceneName, out Scene scene) && scene != null)
            sceneChanger.SwitchSceneAnimation(scene, animationIndex);
    }

    void ParseToScene(Match match)
    {
        string sceneName = match.Groups[1].Value;
        int animationIndex = -1;
        if (match.Groups.Count > 2 && match.Groups[2].Success)
            int.TryParse(match.Groups[2].Value.Trim(), out animationIndex);

        GoToScene(sceneName, animationIndex);
    }

    void ParseMiniGame(Match match, out string gameName, out string sceneName, out int animationIndex)
    {
        gameName = match.Groups[1].Value;
        sceneName = "";

        if (match.Groups.Count > 2 && match.Groups[2].Success)
            sceneName = match.Groups[2].Value;

        animationIndex = -1;
        if (match.Groups.Count > 3 && match.Groups[3].Success)
            int.TryParse(match.Groups[3].Value.Trim(), out animationIndex);
    }

    void ParseStartMiniGame(Match match)
    {
        ParseMiniGame(match, out string gameName, out string sceneName, out int animationIndex);

        switch (gameName)
        {
            case "puzzle":
                puzzleManager.StartPuzzle();
                GoToScene(sceneName, animationIndex);
                break;

            default:
                break;
        }
    }

    void ParseRestartMiniGame(Match match)
    {
        ParseMiniGame(match, out string gameName, out string sceneName, out int animationIndex);

        switch (gameName)
        {
            case "puzzle":
                puzzleManager.RestartPuzzle();
                GoToScene(sceneName, animationIndex);
                break;

            default:
                break;
        }
    }
}
