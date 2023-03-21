using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class LangtonsAnteaterScript : MonoBehaviour
{
    static int _moduleIDCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Cells;
    public SpriteRenderer Ant;
    public SpriteRenderer Anteater;

    private Coroutine StrikeCoroutine;
    private int Answer;
    private bool[] Board = new bool[25];
    private bool Solved;

    void Awake()
    {
        _moduleID = _moduleIDCounter++;
        for (int i = 0; i < Cells.Length; i++)
        {
            int x = i;
            Cells[x].OnInteract += delegate { CellPress(x); return false; };
        }
        Anteater.color = new Color();
        Calculate();
    }

    void CellPress(int pos)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Cells[pos].AddInteractionPunch(.5f);
        if (!Solved)
        {
            if (Answer == pos)
            {
                Module.HandlePass();
                Debug.LogFormat("[Langton's Ant #{0}] You pressed cell {1}, which was correct. Module solved!", _moduleID, new[] { "A", "B", "C", "D", "E" }[pos % 5] + ((pos / 5) + 1).ToString());
                Solved = true;
                if (StrikeCoroutine != null)
                    StopCoroutine(StrikeCoroutine);
                Ant.color = new Color(0, 0, 0, 1);
                StartCoroutine(ShowSolution());
            }
            else
            {
                Debug.LogFormat("[Langton's Ant #{0}] You pressed cell {1}, which was incorrect. Strike!", _moduleID, new[] { "A", "B", "C", "D", "E" }[pos % 5] + ((pos / 5) + 1).ToString());
                Module.HandleStrike();
                if (StrikeCoroutine != null)
                    StopCoroutine(StrikeCoroutine);
                StrikeCoroutine = StartCoroutine(FlashStrike());
            }
        }
    }

    void Calculate()
    {
        for (int i = 0; i < 25; i++)
            Board[i] = Rnd.Range(0, 2) == 1;
        var board = new bool[25];
        for (int i = 0; i < 25; i++)
            board[i] = Board[i];
        int antPos = 12;
        int anteatPos = 12;
        int antDir = 0;
        int anteatDir = 0;
        antDir = (antDir + CheckCell(antPos, board)) % 4;
        board[antPos] = !board[antPos];
        antPos = MoveOneSpace(antPos, antDir);
        int j = 1;
        List<List<string>> logs = new List<List<string>>();
        logs.Add(new List<string> { j.ToString(), new[] { "A", "B", "C", "D", "E" }[antPos % 5] + ((antPos / 5) + 1).ToString(), new[] { "North", "East", "South", "West" }[antDir] });
        while (antPos != anteatPos)
        {
            j++;
            if (j > 32)
                break;
            antDir = (antDir + CheckCell(antPos, board)) % 4;
            anteatDir = (anteatDir + CheckCell(anteatPos, board)) % 4;
            board[antPos] = !board[antPos];
            board[anteatPos] = !board[anteatPos];
            antPos = MoveOneSpace(antPos, antDir);
            anteatPos = MoveOneSpace(anteatPos, anteatDir);
            logs.Add(new List<string> { j.ToString(), new[] { "A", "B", "C", "D", "E" }[antPos % 5] + ((antPos / 5) + 1).ToString(), new[] { "A", "B", "C", "D", "E" }[anteatPos % 5] + ((anteatPos / 5) + 1).ToString(), new[] { "North", "East", "South", "West" }[antDir], new[] { "North", "East", "South", "West" }[anteatDir] });
        }
        if (j <= 32 && j >= 16)
        {
            Answer = antPos;
            Debug.LogFormat("[Langton's Anteater #{0}] Initial board:{1}", _moduleID, board.Select((x, ix) => (ix % 5 == 0 ? "\n" : "") + (x ? "K" : "W")).Join(" "));
            Debug.LogFormat("[Langton's Anteater #{0}] Gen {1}: Ant moved to {2}, anteater has spawned at C3. Ant is now facing {3} and anteater is now facing North.", _moduleID, logs[0][0], logs[0][1], logs[0][2]);
            logs.RemoveAt(0);
            foreach (var log in logs)
                Debug.LogFormat("[Langton's Anteater #{0}] Gen {1}: Ant moved to {2}, anteater moved to {3}. Ant is now facing {4} and anteater is now facing {5}.", _moduleID, log[0], log[1], log[2], log[3], log[4]);
            Debug.LogFormat("[Langton's Anteater #{0}] The anteater ate the ant at {1}, in gen {2}.", _moduleID, new[] { "A", "B", "C", "D", "E" }[antPos % 5] + ((antPos / 5) + 1).ToString(), j.ToString());
            for (int k = 0; k < 25; k++)
                Cells[k].GetComponent<MeshRenderer>().material.color = new[] { new Color(1, 1, 1), new Color(0.25f, 0.25f, 0.25f) }[Board[k] ? 1 : 0];
        }
        else
            Calculate();
    }

    int CheckCell(int pos, bool[] board)
    {
        if (!board[pos])
            return 1;
        return 3;
    }

    int MoveOneSpace(int pos, int dir)
    {
        switch (dir)
        {
            case 0:
                return (pos + 20) % 25;
            case 1:
                if (pos % 5 == 4)
                    return pos - 4;
                else
                    return pos + 1;
            case 2:
                return (pos + 5) % 25;
            default:
                if (pos % 5 == 0)
                    return pos + 4;
                else
                    return pos - 1;
        }
    }

    IEnumerator FlashStrike()
    {
        Ant.color = new Color(1, 0, 0);
        float timer = 0;
        while (timer < 1f)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        Ant.color = new Color(0, 0, 0);
    }

    IEnumerator ShowSolution(float duration = 1 / 6f)
    {
        var board = new bool[25];
        for (int i = 0; i < 25; i++)
            board[i] = Board[i];
        int antPos = 12;
        int anteatPos = 12;
        int antDir = 0;
        int anteatDir = 0;
        int prevAntPos = antPos;
        int prevAnteatPos = anteatPos;
        int prevAntDir = antDir;
        int prevAnteatDir = anteatDir;
        antDir = (antDir + CheckCell(antPos, board)) % 4;
        board[prevAntPos] = !board[prevAntPos];
        antPos = MoveOneSpace(antPos, antDir);
        Color[] colours = new[] { new Color(1, 1, 1), new Color(0.25f, 0.25f, 0.25f) };
        float timer = 0;
        int antEndRot = (prevAntDir == 0 && antDir == 3) ? (antDir * 90) - 360 : (prevAntDir == 3 && antDir == 0) ? (antDir * 90) + 360 : antDir * 90;
        int anteatEndRot = (prevAnteatDir == 0 && anteatDir == 3) ? (anteatDir * 90) - 360 : (prevAnteatDir == 3 && anteatDir == 0) ? (anteatDir * 90) + 360 : anteatDir * 90;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            Ant.transform.localEulerAngles = new Vector3(90, Mathf.Lerp(prevAntDir * 90, antEndRot, timer / duration), 0);
        }
        Cells[prevAntPos].GetComponent<MeshRenderer>().material.color = colours[1 - Array.IndexOf(colours, Cells[prevAntPos].GetComponent<MeshRenderer>().material.color)];
        Audio.PlaySoundAtTransform("move", Ant.transform);
        timer = 0;
        while (timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            Ant.transform.localPosition = new Vector3(Mathf.Lerp((prevAntPos % 5 * 0.03f) - 0.06f, (antPos % 5 * 0.03f) - 0.06f, timer / duration), 0.017f, Mathf.Lerp((prevAntPos / 5 * 0.03f) - 0.06f, (antPos / 5 * 0.03f) - 0.06f, timer / duration));
            Anteater.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, timer / duration));
        }
        Anteater.color = new Color(0, 0, 0, 1);
        while (antPos != anteatPos)
        {
            prevAntPos = antPos;
            prevAnteatPos = anteatPos;
            prevAntDir = antDir;
            prevAnteatDir = anteatDir;
            antDir = (antDir + CheckCell(antPos, board)) % 4;
            anteatDir = (anteatDir + CheckCell(anteatPos, board)) % 4;
            antEndRot = (prevAntDir == 0 && antDir == 3) ? (antDir * 90) - 360 : (prevAntDir == 3 && antDir == 0) ? (antDir * 90) + 360 : antDir * 90;
            anteatEndRot = (prevAnteatDir == 0 && anteatDir == 3) ? (anteatDir * 90) - 360 : (prevAnteatDir == 3 && anteatDir == 0) ? (anteatDir * 90) + 360 : anteatDir * 90;
            board[prevAntPos] = !board[prevAntPos];
            board[prevAnteatPos] = !board[prevAnteatPos];
            antPos = MoveOneSpace(antPos, antDir);
            anteatPos = MoveOneSpace(anteatPos, anteatDir);
            timer = 0;
            while (timer < duration)
            {
                yield return null;
                timer += Time.deltaTime;
                Ant.transform.localEulerAngles = new Vector3(90, Mathf.Lerp(prevAntDir * 90, antEndRot, timer / duration), 0);
                Anteater.transform.localEulerAngles = new Vector3(90, Mathf.Lerp(prevAnteatDir * 90, anteatEndRot, timer / duration), 0);
            }
            Ant.transform.localEulerAngles = new Vector3(90, antEndRot, 0);
            Anteater.transform.localEulerAngles = new Vector3(90, anteatEndRot, 0);
            Cells[prevAntPos].GetComponent<MeshRenderer>().material.color = colours[1 - Array.IndexOf(colours, Cells[prevAntPos].GetComponent<MeshRenderer>().material.color)];
            Cells[prevAnteatPos].GetComponent<MeshRenderer>().material.color = colours[1 - Array.IndexOf(colours, Cells[prevAnteatPos].GetComponent<MeshRenderer>().material.color)];
            SpriteRenderer newAnt = null;
            SpriteRenderer newAnteat = null;
            Vector3 newAntOldPos = new Vector3();
            Vector3 newAnteatOldPos = new Vector3();
            if (WalkingOffBoard(prevAntPos, antDir))
            {
                newAnt = Instantiate(Ant, Ant.transform.parent);
                newAnt.transform.localPosition += new Vector3(new[] { 0, -0.15f, 0, 0.15f }[antDir], 0, new[] { -0.15f, 0, 0.15f, 0 }[antDir]);
                newAntOldPos = newAnt.transform.localPosition;
                newAnt.color = new Color();
            }
            if (WalkingOffBoard(prevAnteatPos, anteatDir))
            {
                newAnteat = Instantiate(Anteater, Anteater.transform.parent);
                newAnteat.transform.localPosition += new Vector3(new[] { 0, -0.15f, 0, 0.15f }[anteatDir], 0, new[] { -0.15f, 0, 0.15f, 0 }[anteatDir]);
                newAnteatOldPos = newAnteat.transform.localPosition;
                newAnteat.color = new Color();
            }
            Audio.PlaySoundAtTransform("move", Ant.transform);
            Audio.PlaySoundAtTransform("move", Anteater.transform);
            timer = 0;
            while (timer < duration)
            {
                yield return null;
                timer += Time.deltaTime;
                Ant.transform.localPosition = new Vector3(Mathf.Lerp((prevAntPos % 5 * 0.03f) - 0.06f, (antPos % 5 * 0.03f) - 0.06f + (newAnt != null ? new[] { 0, 0.15f, 0, -0.15f }[antDir] : 0), timer / duration), 0.017f, -Mathf.Lerp((prevAntPos / 5 * 0.03f) - 0.06f, (antPos / 5 * 0.03f) - 0.06f - (newAnt != null ? new[] { 0.15f, 0, -0.15f, 0 }[antDir] : 0), timer / duration));
                Anteater.transform.localPosition = new Vector3(Mathf.Lerp((prevAnteatPos % 5 * 0.03f) - 0.06f, (anteatPos % 5 * 0.03f) - 0.06f + (newAnteat != null ? new[] { 0, 0.15f, 0, -0.15f }[anteatDir] : 0), timer / duration), 0.017f, -Mathf.Lerp((prevAnteatPos / 5 * 0.03f) - 0.06f, (anteatPos / 5 * 0.03f) - 0.06f - (newAnteat != null ? new[] { 0.15f, 0, -0.15f, 0 }[anteatDir] : 0), timer / duration));
                if (newAnt != null)
                {
                    newAnt.transform.localPosition = newAntOldPos + new Vector3(new[] { 0, 0.03f, 0, -0.03f }[antDir], 0, new[] { 0.03f, 0, -0.03f, 0 }[antDir]) * Mathf.Lerp(0, 1f, timer / duration);
                    newAnt.color = new Color(0, 0, 0, Mathf.Lerp(0, 1f, timer / duration));
                    Ant.color = new Color(0, 0, 0, Mathf.Lerp(1f, 0, timer / duration));
                }
                if (newAnteat != null)
                {
                    newAnteat.transform.localPosition = newAnteatOldPos + new Vector3(new[] { 0, 0.03f, 0, -0.03f }[anteatDir], 0, new[] { 0.03f, 0, -0.03f, 0 }[anteatDir]) * Mathf.Lerp(0, 1f, timer / duration);
                    newAnteat.color = new Color(0, 0, 0, Mathf.Lerp(0, 1f, timer / duration));
                    Anteater.color = new Color(0, 0, 0, Mathf.Lerp(1f, 0, timer / duration));
                }
            }
            Ant.transform.localPosition = new Vector3((antPos % 5 * 0.03f) - 0.06f, 0.017f, (antPos / 5 * -0.03f) + 0.06f);
            Ant.color = new Color(0, 0, 0, 1);
            Anteater.transform.localPosition = new Vector3((anteatPos % 5 * 0.03f) - 0.06f, 0.017f, (anteatPos / 5 * -0.03f) + 0.06f);
            Anteater.color = new Color(0, 0, 0, 1);
            if (newAnt != null)
                Destroy(newAnt.gameObject);
            if (newAnteat != null)
                Destroy(newAnteat.gameObject);
        }
        Anteater.transform.localEulerAngles = new Vector3(90, anteatDir * 90, 0);
        Anteater.transform.localPosition = new Vector3((anteatPos % 5 * 0.03f) - 0.06f, 0.017f, (anteatPos / 5 * -0.03f) + 0.06f);
        Ant.color = new Color();
        Anteater.color = new Color(0, 1, 0);
        Audio.PlaySoundAtTransform("eat", Anteater.transform);
    }

    bool WalkingOffBoard(int pos, int dir)
    {
        switch (dir)
        {
            case 0:
                if (pos / 5 == 0)
                    return true;
                return false;
            case 1:
                if (pos % 5 == 4)
                    return true;
                return false;
            case 2:
                if (pos / 5 == 4)
                    return true;
                return false;
            default:
                if (pos % 5 == 0)
                    return true;
                return false;
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "Use '!{0} b4' to press the cell at B4.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string[] coords = new[] { "a1", "b1", "c1", "d1", "e1", "a2", "b2", "c2", "d2", "e2", "a3", "b3", "c3", "d3", "e3", "a4", "b4", "c4", "d4", "e4", "a5", "b5", "c5", "d5", "e5" };
        if (!coords.Contains(command))
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
        yield return null;
        Cells[Array.IndexOf(coords, command)].OnInteract();
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return true;
        Cells[Answer].OnInteract();
    }
}
