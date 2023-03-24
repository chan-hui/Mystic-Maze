using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Text.RegularExpressions;

public class mysticmazeScript : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombInfo bomb;
    public KMBombModule Module;

    public KMSelectable Display;
    public KMSelectable[] Arrows;

    public GameObject DisplayMove;
    public GameObject DisplayKey;
    public Material[] morseMat;
    public Material[] keyMat;

    public GameObject[] Displays;

    private List<char> MappedLetters = new List<char>();
    private char[,] Maze = new char[19, 19]{
        {'W',   'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W'},
        {'W',   'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W'},
        {'W',   'W',    'U',    'H',    'U',    'H',    'U',    'H',    'U',    'H',    'U',    'H',    'U',    'H',    'U',    'H',    'U',    'W',    'W'},
        {'W',   'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'W'},
        {'W',   'W',    'U',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'U',    'W',    'W'},
        {'W',   'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'W'},
        {'W',   'W',    'U',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'U',    'W',    'W'},
        {'W',   'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'W'},
        {'W',   'W',    'U',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'U',    'W',    'W'},
        {'W',   'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'W'},
        {'W',   'W',    'U',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'U',    'W',    'W'},
        {'W',   'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'W'},
        {'W',   'W',    'U',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'U',    'W',    'W'},
        {'W',   'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'W'},
        {'W',   'W',    'U',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'S',    'H',    'U',    'W',    'W'},
        {'W',   'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'H',    'W',    'W'},
        {'W',   'W',    'U',    'H',    'U',    'H',    'U',    'H',    'U',    'H',    'U',    'H',    'U',    'H',    'U',    'H',    'U',    'W',    'W'},
        {'W',   'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W'},
        {'W',   'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W',    'W'}
    };
    private int currentR;
    private int currentC;

    private bool pickedKey1;
    private bool pickedKey2;


    //logging
    static int moduleIdCounter = 1;
    int moduleId;

    private bool moduleSolved;
    private bool animationPlaying;
    private bool morsePlaying;
    private string Mazerender;

    void Awake()
    {
        moduleId = moduleIdCounter++;

        Display.OnInteract += delegate () { displayPress(); return false; };
        foreach (KMSelectable arrow in Arrows)
        {
            KMSelectable pressedArrow = arrow;
            for (int i = 0; i < Arrows.Length; i++)
            {
                if (pressedArrow == Arrows[i])
                {
                    arrow.OnInteract += delegate () { arrowPress(pressedArrow, i); return false; };
                    break;
                }
            }
        }
    }

    void Start()
    {
        initialMapping();
        generateMaze();
        updateDisplay();
    }

    void initialMapping()
    {
        int initialLetter = UnityEngine.Random.Range('A', 'Z' + 1);
        MappedLetters.Add((char)initialLetter);

        string serialNo = bomb.GetSerialNumber();
        List<int> mappingIndex = new List<int>();
        foreach (var c in serialNo)
            mappingIndex.Add((c >= '0' && c <= '9') ? (c - '0') : (c - 'A' + 1));

        for (int i = 0; MappedLetters.Count < 19; i++)
        {
            initialLetter += mappingIndex[i % 6];
            for (; initialLetter > 'Z';)
                initialLetter -= 26;
            while (MappedLetters.Contains((char)initialLetter))
            {
                initialLetter += 1;
                for (; initialLetter > 'Z';)
                    initialLetter -= 26;
            }
            MappedLetters.Add((char)initialLetter);
        }

        string index = "";
        for (int i = 1; i < MappedLetters.Count(); i++)
            index += MappedLetters[i];

        Debug.LogFormat("[Mystic Maze #{0}] Initial Character is {1}, Mapped Characters are {2}.", moduleId, MappedLetters[0], index);
    }

    void generateMaze()
    {
        List<int> SR = new List<int>(new int[] { 4, 6, 8, 10, 12, 14 });
        List<int> SC = new List<int>(new int[] { 4, 6, 8, 10, 12, 14 });
        SR.Shuffle();
        SC.Shuffle();

        currentR = SR[0];
        currentC = SC[0];

        Maze[SR[0], SC[0]] = 'I';
        MazeMaster(SR[0], SC[0], "UDLR", false);
        Maze[SR[1], SC[1]] = 'K';
        MazeMaster(SR[1], SC[1], "", true);
        Maze[SR[2], SC[2]] = 'Y';
        MazeMaster(SR[2], SC[2], "", true);
        Maze[SR[3], SC[3]] = 'E';
        MazeMaster(SR[3], SC[3], "", true);

        int curDr = 0, curDc = 0;
        for (; Maze[curDr, curDc] != 'U';)
        {
            curDr = Rnd.Range(2, 17);
            curDc = Rnd.Range(2, 17);
        }
        Maze[curDr, curDc] = 'D';

        for (int lp = 0; lp < 100; lp++)
        {
            for (; ; )
            {
                List<char> validMove = new List<char>();
                if (!(Maze[curDr - 1, curDc] == 'P' || Maze[curDr - 1, curDc] == 'W' || Maze[curDr - 2, curDc] == 'D'))
                    validMove.Add('U');
                if (!(Maze[curDr + 1, curDc] == 'P' || Maze[curDr + 1, curDc] == 'W' || Maze[curDr + 2, curDc] == 'D'))
                    validMove.Add('D');
                if (!(Maze[curDr, curDc - 1] == 'P' || Maze[curDr, curDc - 1] == 'W' || Maze[curDr, curDc - 2] == 'D'))
                    validMove.Add('L');
                if (!(Maze[curDr, curDc + 1] == 'P' || Maze[curDr, curDc + 1] == 'W' || Maze[curDr, curDc + 2] == 'D'))
                    validMove.Add('R');
                if (validMove.Count() == 0)
                    break;
                char Move = validMove[Rnd.Range(0, validMove.Count())];
                if (Move == 'U')
                {
                    Maze[curDr - 1, curDc] = 'P';
                    Maze[curDr - 2, curDc] = 'D';
                    curDr -= 2;
                }
                if (Move == 'D')
                {
                    Maze[curDr + 1, curDc] = 'P';
                    Maze[curDr + 2, curDc] = 'D';
                    curDr += 2;
                }
                if (Move == 'L')
                {
                    Maze[curDr, curDc - 1] = 'P';
                    Maze[curDr, curDc - 2] = 'D';
                    curDc -= 2;
                }
                if (Move == 'R')
                {
                    Maze[curDr, curDc + 1] = 'P';
                    Maze[curDr, curDc + 2] = 'D';
                    curDc += 2;
                }
            }

            bool mazefinishVar = true;
            foreach (char c in Maze)
                if (c == 'U' || c == 'S')
                    mazefinishVar = false;
            if (mazefinishVar)
                break;

            for (; !((Maze[curDr, curDc] == 'U' || Maze[curDr, curDc] == 'S') && (Maze[curDr - 2, curDc] == 'D' || Maze[curDr + 2, curDc] == 'D' || Maze[curDr, curDc - 2] == 'D' || Maze[curDr, curDc + 2] == 'D'));)
            {
                curDr = Rnd.Range(2, 17);
                curDc = Rnd.Range(2, 17);
            }
            Maze[curDr, curDc] = 'D';
            List<char> validAdj = new List<char>();
            if (Maze[curDr - 2, curDc] == 'D')
                validAdj.Add('U');
            if (Maze[curDr + 2, curDc] == 'D')
                validAdj.Add('D');
            if (Maze[curDr, curDc - 2] == 'D')
                validAdj.Add('L');
            if (Maze[curDr, curDc + 2] == 'D')
                validAdj.Add('R');
            char Adj = validAdj[Rnd.Range(0, validAdj.Count())];
            if (Adj == 'U')
                Maze[curDr - 1, curDc] = 'P';
            if (Adj == 'D')
                Maze[curDr + 1, curDc] = 'P';
            if (Adj == 'L')
                Maze[curDr, curDc - 1] = 'P';
            if (Adj == 'R')
                Maze[curDr, curDc + 1] = 'P';
        }

        for (int i = 1; i < 18; i++)
            for (int j = 1; j < 18; j++)
                if (Maze[i, j] == 'H')
                    Maze[i, j] = 'W';

        Mazerender = "";
        for (int i = 1; i < 18; i++)
        {
            for (int j = 1; j < 18; j++)
            {
                if (Maze[i, j] == 'W')
                    Mazerender += '■';
                if (Maze[i, j] == 'D' || Maze[i, j] == 'P')
                    Mazerender += '□';
                if (Maze[i, j] == 'I' || Maze[i, j] == 'K' || Maze[i, j] == 'Y' || Maze[i, j] == 'E')
                    Mazerender += Maze[i, j];
            }
            Mazerender += '\n';
        }
        Debug.LogFormat("[Mystic Maze #{0}] Generated Maze is \n{1}(I = Initial position, K = Key 1, Y = keY 2, E = Exit)", moduleId, Mazerender);
    }

    void MazeMaster(int X, int Y, String door, bool onedoor)
    {
        if (!onedoor)
        {
            if (door.Contains('U'))
                Maze[X - 1, Y] = 'P';
            if (door.Contains('D'))
                Maze[X + 1, Y] = 'P';
            if (door.Contains('L'))
                Maze[X, Y - 1] = 'P';
            if (door.Contains('R'))
                Maze[X, Y + 1] = 'P';
        }
        else
        {
            int ranRot = Rnd.Range(0, 4);
            if (ranRot == 0)
            {
                Maze[X - 1, Y] = 'P';
                Maze[X + 1, Y] = 'W'; Maze[X, Y - 1] = 'W'; Maze[X, Y + 1] = 'W';
            }
            if (ranRot == 1)
            {
                Maze[X + 1, Y] = 'P';
                Maze[X - 1, Y] = 'W'; Maze[X, Y - 1] = 'W'; Maze[X, Y + 1] = 'W';
            }
            if (ranRot == 2)
            {
                Maze[X, Y - 1] = 'P';
                Maze[X + 1, Y] = 'W'; Maze[X - 1, Y] = 'W'; Maze[X, Y + 1] = 'W';
            }
            if (ranRot == 3)
            {
                Maze[X, Y + 1] = 'P';
                Maze[X + 1, Y] = 'W'; Maze[X - 1, Y] = 'W'; Maze[X, Y - 1] = 'W';
            }
        }
    }

    void updateDisplay()
    {
        char displayedLetter = '\0';
        if (Maze[currentR, currentC] == 'I')
            displayedLetter = MappedLetters[0];
        else if (Maze[currentR, currentC] == 'K')
            displayedLetter = MappedLetters[16];
        else if (Maze[currentR, currentC] == 'Y')
            displayedLetter = MappedLetters[17];
        else if (Maze[currentR, currentC] == 'E')
            displayedLetter = MappedLetters[18];
        else
        {
            string validDir = "";
            if (Maze[currentR - 1, currentC] == 'P')
                validDir += "U";
            if (Maze[currentR + 1, currentC] == 'P')
                validDir += "D";
            if (Maze[currentR, currentC - 1] == 'P')
                validDir += "L";
            if (Maze[currentR, currentC + 1] == 'P')
                validDir += "R";
            if (validDir == "U")
                displayedLetter = MappedLetters[1];
            if (validDir == "D")
                displayedLetter = MappedLetters[2];
            if (validDir == "L")
                displayedLetter = MappedLetters[3];
            if (validDir == "R")
                displayedLetter = MappedLetters[4];
            if (validDir == "UD")
                displayedLetter = MappedLetters[5];
            if (validDir == "UL")
                displayedLetter = MappedLetters[6];
            if (validDir == "UR")
                displayedLetter = MappedLetters[7];
            if (validDir == "DL")
                displayedLetter = MappedLetters[8];
            if (validDir == "DR")
                displayedLetter = MappedLetters[9];
            if (validDir == "LR")
                displayedLetter = MappedLetters[10];
            if (validDir == "UDL")
                displayedLetter = MappedLetters[11];
            if (validDir == "UDR")
                displayedLetter = MappedLetters[12];
            if (validDir == "ULR")
                displayedLetter = MappedLetters[13];
            if (validDir == "DLR")
                displayedLetter = MappedLetters[14];
            if (validDir == "UDLR")
                displayedLetter = MappedLetters[15];
        }

        ClearDisplay();

        int letterFont = Rnd.Range(0, Displays.Length + 1);
        var pigpenReorder = new int[] { 0, 9, 1, 10, 2, 11, 3, 12, 4, 13, 5, 14, 6, 15, 7, 16, 8, 17, 18, 22, 19, 23, 20, 24, 21, 25 };
        if (letterFont == 0)
        {
            Displays[0].GetComponent<TextMesh>().text = displayedLetter.ToString();
            Debug.LogFormat("[Mystic Maze #{0}] Displayed Letter is {1} in {2}.", moduleId, displayedLetter, "Lombax");
        }
        else if (letterFont == 1)
        {
            Displays[1].GetComponent<TextMesh>().text = displayedLetter.ToString();
            Debug.LogFormat("[Mystic Maze #{0}] Displayed Letter is {1} in {2}.", moduleId, displayedLetter, "Zoni");
        }
        else if (letterFont == 2)
        {
            Displays[2].GetComponent<TextMesh>().text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[Array.IndexOf(pigpenReorder, displayedLetter - 'A')].ToString();
            Debug.LogFormat("[Mystic Maze #{0}] Displayed Letter is {1} in {2}.", moduleId, displayedLetter, "Pigpen");
        }
        else if (letterFont == 3)
        {
            Displays[3].GetComponent<TextMesh>().text = displayedLetter.ToString();
            Debug.LogFormat("[Mystic Maze #{0}] Displayed Letter is {1} in {2}.", moduleId, displayedLetter, "Semaphore");
        }
        else if (letterFont == 4)
        {
            Displays[4].GetComponent<TextMesh>().text = displayedLetter.ToString();
            Debug.LogFormat("[Mystic Maze #{0}] Displayed Letter is {1} in {2}.", moduleId, displayedLetter, "R'lyehian");
        }
        else if (letterFont == 5)
        {
            string Bin = "00000";
            string temBin = Convert.ToString((int)displayedLetter - 'A' + 1, 2);
            Bin = Bin.Remove(Bin.Length - temBin.Length, temBin.Length);
            Bin += temBin;
            Displays[5].GetComponent<TextMesh>().text = Bin;
            Debug.LogFormat("[Mystic Maze #{0}] Displayed Letter is {1} in {2}.", moduleId, displayedLetter, "Binary");
        }
        else if (letterFont == 6)
        {
            morsePlaying = true;
            StartCoroutine(Morse(displayedLetter.ToString()));
            Debug.LogFormat("[Mystic Maze #{0}] Displayed Letter is {1} in {2}.", moduleId, displayedLetter, "Morse");
        }
    }

    IEnumerator Morse(string received)
    {
        string convMorse = "";
        if (received == "A") { convMorse = ".-X"; }
        if (received == "B") { convMorse = "-...X"; }
        if (received == "C") { convMorse = "-.-.X"; }
        if (received == "D") { convMorse = "-..X"; }
        if (received == "E") { convMorse = ".X"; }
        if (received == "F") { convMorse = "..-.X"; }
        if (received == "G") { convMorse = "--.X"; }
        if (received == "H") { convMorse = "....X"; }
        if (received == "I") { convMorse = "..X"; }
        if (received == "J") { convMorse = ".---X"; }
        if (received == "K") { convMorse = "-.-X"; }
        if (received == "L") { convMorse = ".-..X"; }
        if (received == "M") { convMorse = "--X"; }
        if (received == "N") { convMorse = "-.X"; }
        if (received == "O") { convMorse = "---X"; }
        if (received == "P") { convMorse = ".--.X"; }
        if (received == "Q") { convMorse = "--.-X"; }
        if (received == "R") { convMorse = ".-.X"; }
        if (received == "S") { convMorse = "...X"; }
        if (received == "T") { convMorse = "-X"; }
        if (received == "U") { convMorse = "..-X"; }
        if (received == "V") { convMorse = "...-X"; }
        if (received == "W") { convMorse = ".--X"; }
        if (received == "X") { convMorse = "-..-X"; }
        if (received == "Y") { convMorse = "-.--X"; }
        if (received == "Z") { convMorse = "--..X"; }

        if (morsePlaying)
        {
            foreach (char c in convMorse)
            {
                if (c == '.' && morsePlaying)
                {
                    Display.GetComponent<MeshRenderer>().material = morseMat[1];
                    if (morsePlaying)
                        yield return new WaitForSeconds(0.2f);
                    Display.GetComponent<MeshRenderer>().material = morseMat[0];
                    if (morsePlaying)
                        yield return new WaitForSeconds(0.2f);
                }
                if (c == '-' && morsePlaying)
                {
                    Display.GetComponent<MeshRenderer>().material = morseMat[1];
                    yield return new WaitForSeconds(0.2f);
                    if (morsePlaying)
                        yield return new WaitForSeconds(0.2f);
                    if (morsePlaying)
                        yield return new WaitForSeconds(0.2f);
                    Display.GetComponent<MeshRenderer>().material = morseMat[0];
                    yield return new WaitForSeconds(0.2f);
                }
                if (c == 'X' && morsePlaying)
                {
                    yield return new WaitForSeconds(0.2f);
                    if (morsePlaying)
                        yield return new WaitForSeconds(0.2f);
                    if (morsePlaying)
                        yield return new WaitForSeconds(0.2f);
                }
            }
            if (morsePlaying)
                StartCoroutine(Morse(received));
        }
    }


    void arrowPress(KMSelectable arrow, int indv)
    {
        if (moduleSolved || animationPlaying)
            return;
        arrow.AddInteractionPunch(.5f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (indv == 0)
        {
            if (Maze[currentR - 1, currentC] == 'W')
            {
                Module.HandleStrike();
                Debug.LogFormat("[Mystic Maze #{0}] Strike! You ran into a wall when attempting to move up.", moduleId);
            }
            else
            {
                currentR -= 2;
                Debug.LogFormat("[Mystic Maze #{0}] You moved to up.", moduleId);
                StartCoroutine(Moving('U'));
            }
        }
        if (indv == 1)
        {
            if (Maze[currentR + 1, currentC] == 'W')
            {
                Module.HandleStrike();
                Debug.LogFormat("[Mystic Maze #{0}] Strike! You ran into a wall when attempting to move down.", moduleId);
            }
            else
            {
                currentR += 2;
                Debug.LogFormat("[Mystic Maze #{0}] You moved to down.", moduleId);
                StartCoroutine(Moving('D'));
            }
        }
        if (indv == 2)
        {
            if (Maze[currentR, currentC - 1] == 'W')
            {
                Module.HandleStrike();
                Debug.LogFormat("[Mystic Maze #{0}] Strike! You ran into a wall when attempting to move left.", moduleId);
            }
            else
            {
                currentC -= 2;
                Debug.LogFormat("[Mystic Maze #{0}] You moved to left.", moduleId);
                StartCoroutine(Moving('L'));
            }
        }
        if (indv == 3)
        {
            if (Maze[currentR, currentC + 1] == 'W')
            {
                Module.HandleStrike();
                Debug.LogFormat("[Mystic Maze #{0}] Strike! You ran into a wall when attempting to move right.", moduleId);
            }
            else
            {
                currentC += 2;
                Debug.LogFormat("[Mystic Maze #{0}] You moved to right.", moduleId);
                StartCoroutine(Moving('R'));
            }
        }


    }

    void displayPress()
    {
        if (moduleSolved || animationPlaying)
            return;

        Display.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (Maze[currentR, currentC] == 'K')
        {
            if (pickedKey1)
                return;
            pickedKey1 = true;
            Debug.LogFormat("[Mystic Maze #{0}] You picked Key 1.", moduleId);
            StartCoroutine(Key());
        }
        else if (Maze[currentR, currentC] == 'Y')
        {
            if (pickedKey2)
                return;
            pickedKey2 = true;
            Debug.LogFormat("[Mystic Maze #{0}] You picked Key 2.", moduleId);
            StartCoroutine(Key());
        }
        else if (Maze[currentR, currentC] == 'E' && pickedKey1 && pickedKey2)
        {
            moduleSolved = true;
            Module.HandlePass();
            Debug.LogFormat("[Mystic Maze #{0}] You opened exit with keys, Module solved.", moduleId);
            StartCoroutine(Solve());
        }
        else if (Maze[currentR, currentC] == 'E')
        {
            Module.HandleStrike();
            Debug.LogFormat("[Mystic Maze #{0}] Strike! You don't have keys.", moduleId);
        }
        else
        {
            Module.HandleStrike();
            Debug.LogFormat("[Mystic Maze #{0}] Strike! You pressed display on nothing.", moduleId);
        }
    }

    IEnumerator Moving(char dir)
    {
        animationPlaying = true;
        ClearDisplay();

        if (dir == 'U')
        {
            DisplayMove.GetComponent<TextMesh>().text = "△\n△\n▲";
            yield return new WaitForSeconds(0.1f);
            DisplayMove.GetComponent<TextMesh>().text = "△\n▲\n△";
            yield return new WaitForSeconds(0.1f);
            DisplayMove.GetComponent<TextMesh>().text = "▲\n△\n△";
            yield return new WaitForSeconds(0.1f);
        }
        if (dir == 'D')
        {
            DisplayMove.GetComponent<TextMesh>().text = "▼\n▽\n▽";
            yield return new WaitForSeconds(0.1f);
            DisplayMove.GetComponent<TextMesh>().text = "▽\n▼\n▽";
            yield return new WaitForSeconds(0.1f);
            DisplayMove.GetComponent<TextMesh>().text = "▽\n▽\n▼";
            yield return new WaitForSeconds(0.1f);
        }
        if (dir == 'L')
        {
            DisplayMove.GetComponent<TextMesh>().text = "◁◁◀";
            yield return new WaitForSeconds(0.1f);
            DisplayMove.GetComponent<TextMesh>().text = "◁◀◁";
            yield return new WaitForSeconds(0.1f);
            DisplayMove.GetComponent<TextMesh>().text = "◀◁◁";
            yield return new WaitForSeconds(0.1f);
        }
        if (dir == 'R')
        {
            DisplayMove.GetComponent<TextMesh>().text = "▶▷▷";
            yield return new WaitForSeconds(0.1f);
            DisplayMove.GetComponent<TextMesh>().text = "▷▶▷";
            yield return new WaitForSeconds(0.1f);
            DisplayMove.GetComponent<TextMesh>().text = "▷▷▶";
            yield return new WaitForSeconds(0.1f);
        }
        DisplayMove.GetComponent<TextMesh>().text = "";
        animationPlaying = false;

        updateDisplay();

    }

    IEnumerator Key()
    {
        animationPlaying = true;
        ClearDisplay();
        Audio.PlaySoundAtTransform("sfx3", transform);

        DisplayKey.GetComponent<Transform>().localScale = new Vector3(1, (float)0.000001, 1);
        DisplayKey.GetComponent<MeshRenderer>().material = keyMat[0];
        yield return new WaitForSeconds(0.2f);
        DisplayKey.GetComponent<MeshRenderer>().material = keyMat[1];
        yield return new WaitForSeconds(0.2f);
        DisplayKey.GetComponent<MeshRenderer>().material = keyMat[0];
        yield return new WaitForSeconds(0.2f);
        DisplayKey.GetComponent<MeshRenderer>().material = keyMat[1];
        yield return new WaitForSeconds(0.2f);
        DisplayKey.GetComponent<Transform>().localScale = new Vector3(0, (float)0.000001, 1);

        animationPlaying = false;
        updateDisplay();
    }

    IEnumerator Solve()
    {
        animationPlaying = true;
        ClearDisplay();
        Audio.PlaySoundAtTransform("sfx1", transform);

        DisplayKey.GetComponent<Transform>().localScale = new Vector3(1, (float)0.000001, 1);
        DisplayKey.GetComponent<MeshRenderer>().material = keyMat[2];
        yield return new WaitForSeconds(0.2f);
    }

    void ClearDisplay()
    {
        for (int i = 0; i < Displays.Length; i++)
        {
            Displays[i].GetComponent<TextMesh>().text = "";
        }
        morsePlaying = false;
        DisplayMove.GetComponent<TextMesh>().text = "";
        DisplayKey.GetComponent<Transform>().localScale = new Vector3(0, (float)0.000001, 1);
    }

#pragma warning disable 0414
    private string TwitchHelpMessage = "Move using either !{0} U R D L or !{0} N E S W. Press the center screen using either !{0} C or !{0} M. Commands can be chained with spaces, semicolons, or commas.";
#pragma warning restore 0414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        var m = Regex.Match(command, @"^\s*([urdlneswmc,; ]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!m.Success)
            yield break;
        yield return null;
        foreach (var ch in m.Groups[1].Value)
        {
            var c = ch.ToString().ToLowerInvariant();
            if (c == " " || c == ";" || c == ",")
                continue;
            if (c == "n" || c == "u")
                Arrows[0].OnInteract();
            else if (c == "e" || c == "r")
                Arrows[3].OnInteract();
            else if (c == "s" || c == "d")
                Arrows[1].OnInteract();
            else if (c == "w" || c == "l")
                Arrows[2].OnInteract();
            else if (c == "m" || c == "c")
                Display.OnInteract();
            while (animationPlaying)
                yield return null;
            yield return new WaitForSeconds(0.1f);
        }
    }

    struct QueueItem
    {
        public int Cell;
        public int Parent;
        public int Direction;
        public QueueItem(int cell, int parent, int dir)
        {
            Cell = cell;
            Parent = parent;
            Direction = dir;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        var maze = Mazerender.Split('\n').Join("");
        if (!pickedKey1)
        {
            var current = (currentR - 1) * 17 + (currentC - 1);
            var visited = new Dictionary<int, QueueItem>();
            var q = new Queue<QueueItem>();
            var sol = maze.IndexOf('K');
            q.Enqueue(new QueueItem(current, -1, 0));
            while (q.Count > 0)
            {
                var qi = q.Dequeue();
                if (visited.ContainsKey(qi.Cell))
                    continue;
                visited[qi.Cell] = qi;
                if (qi.Cell == sol)
                    break;
                if (maze[qi.Cell - 17] != '■')
                    q.Enqueue(new QueueItem(qi.Cell - 34, qi.Cell, 0));
                if (maze[qi.Cell + 1] != '■')
                    q.Enqueue(new QueueItem(qi.Cell + 2, qi.Cell, 3));
                if (maze[qi.Cell + 17] != '■')
                    q.Enqueue(new QueueItem(qi.Cell + 34, qi.Cell, 1));
                if (maze[qi.Cell - 1] != '■')
                    q.Enqueue(new QueueItem(qi.Cell - 2, qi.Cell, 2));
            }
            var r = sol;
            var path = new List<int>();
            while (true)
            {
                var nr = visited[r];
                if (nr.Parent == -1)
                    break;
                path.Add(nr.Direction);
                r = nr.Parent;
            }
            for (int i = path.Count - 1; i >= 0; i--)
            {
                Arrows[path[i]].OnInteract();
                while (animationPlaying)
                    yield return null;
                yield return new WaitForSeconds(0.1f);
            }
            Display.OnInteract();
            while (animationPlaying)
                yield return null;
        }
        if (!pickedKey2)
        {
            var current = (currentR - 1) * 17 + (currentC - 1);
            var visited = new Dictionary<int, QueueItem>();
            var q = new Queue<QueueItem>();
            var sol = maze.IndexOf('Y');
            q.Enqueue(new QueueItem(current, -1, 0));
            while (q.Count > 0)
            {
                var qi = q.Dequeue();
                if (visited.ContainsKey(qi.Cell))
                    continue;
                visited[qi.Cell] = qi;
                if (qi.Cell == sol)
                    break;
                if (maze[qi.Cell - 17] != '■')
                    q.Enqueue(new QueueItem(qi.Cell - 34, qi.Cell, 0));
                if (maze[qi.Cell + 1] != '■')
                    q.Enqueue(new QueueItem(qi.Cell + 2, qi.Cell, 3));
                if (maze[qi.Cell + 17] != '■')
                    q.Enqueue(new QueueItem(qi.Cell + 34, qi.Cell, 1));
                if (maze[qi.Cell - 1] != '■')
                    q.Enqueue(new QueueItem(qi.Cell - 2, qi.Cell, 2));
            }
            var r = sol;
            var path = new List<int>();
            while (true)
            {
                var nr = visited[r];
                if (nr.Parent == -1)
                    break;
                path.Add(nr.Direction);
                r = nr.Parent;
            }
            for (int i = path.Count - 1; i >= 0; i--)
            {
                Arrows[path[i]].OnInteract();
                while (animationPlaying)
                    yield return null;
                yield return new WaitForSeconds(0.1f);
            }
            Display.OnInteract();
            while (animationPlaying)
                yield return null;
        }
        if (pickedKey1 && pickedKey2)
        {
            var current = (currentR - 1) * 17 + (currentC - 1);
            var visited = new Dictionary<int, QueueItem>();
            var q = new Queue<QueueItem>();
            var sol = maze.IndexOf('E');
            q.Enqueue(new QueueItem(current, -1, 0));
            while (q.Count > 0)
            {
                var qi = q.Dequeue();
                if (visited.ContainsKey(qi.Cell))
                    continue;
                visited[qi.Cell] = qi;
                if (qi.Cell == sol)
                    break;
                if (maze[qi.Cell - 17] != '■')
                    q.Enqueue(new QueueItem(qi.Cell - 34, qi.Cell, 0));
                if (maze[qi.Cell + 1] != '■')
                    q.Enqueue(new QueueItem(qi.Cell + 2, qi.Cell, 3));
                if (maze[qi.Cell + 17] != '■')
                    q.Enqueue(new QueueItem(qi.Cell + 34, qi.Cell, 1));
                if (maze[qi.Cell - 1] != '■')
                    q.Enqueue(new QueueItem(qi.Cell - 2, qi.Cell, 2));
            }
            var r = sol;
            var path = new List<int>();
            while (true)
            {
                var nr = visited[r];
                if (nr.Parent == -1)
                    break;
                path.Add(nr.Direction);
                r = nr.Parent;
            }
            for (int i = path.Count - 1; i >= 0; i--)
            {
                Arrows[path[i]].OnInteract();
                while (animationPlaying)
                    yield return null;
                yield return new WaitForSeconds(0.1f);
            }
            Display.OnInteract();
            while (animationPlaying && !moduleSolved)
                yield return null;
        }
        yield break;
    }
}
