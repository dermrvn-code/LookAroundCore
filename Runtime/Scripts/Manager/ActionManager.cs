using System.Text.RegularExpressions;
using UnityEngine;

public enum ActionType
{
    None,
    ToScene,
    StartMiniGame,
    RestartMiniGame
}

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


    string toScenePattern = @"\btoScene\(([^,)]+?)(?:,(-?\d))*\)";
    string startMiniGamePattern = @"\bstartMiniGame\(([^,)]+?)(?:,([^,)]+?))?(?:,(-?\d))?\)";
    string restartMiniGamePattern = @"\brestartMiniGame\(([^,)]+?)(?:,([^,)]+?))?(?:,(-?\d))?\)";

    public void ActionParser(string action)
    {
        switch (GetFunction(action, out Match match))
        {
            case ActionType.ToScene:
                ParseToScene(match);
                break;
            case ActionType.StartMiniGame:
                ParseStartMiniGame(match);
                break;
            case ActionType.RestartMiniGame:
                ParseRestartMiniGame(match);
                break;
            default:
                break;
        }
    }

    public ActionType GetFunction(string action, out Match match)
    {
        match = null;

        if (string.IsNullOrEmpty(action)) return ActionType.None;

        // TO SCENE
        match = Regex.Match(action, toScenePattern);
        if (match.Success)
        {
            return ActionType.ToScene;
        }

        // START MINIGAME
        match = Regex.Match(action, startMiniGamePattern);
        if (match.Success)
        {
            return ActionType.StartMiniGame;
        }

        // RESTART MINIGAME
        match = Regex.Match(action, restartMiniGamePattern);
        if (match.Success)
        {
            return ActionType.RestartMiniGame;
        }

        Debug.Log($"Could not parse '{action}'");
        return ActionType.None;
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
