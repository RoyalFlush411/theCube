using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using theCube;

public class theCubeScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public Renderer cipherBox;
    public KMSelectable[] numberButtons;
    public KMSelectable executeButton;
    public AudioClip[] beeps;
    private int beepIndex = 0;

    //Stage Counter
    public Renderer stageWheel;
    public Renderer logoWheel;
    public Renderer[] lights;
    public Texture[] lightColours;

    //Cube movement
    int rotation = 0;
    int axisSelection = 0;
    int selectionIncreaser = 0;
    private List<int> selectedRotations = new List<int>();
    public string[] rotationName;
    private bool moduleSolved = false;
    private bool rotationComplete = false;

    //Cube screens
    public TextMesh[] cubeScreens;
    private List<int> cubeNumbers = new List<int>();

    //Wires
    public Texture[] colourOptions;
    public Renderer[] wire1;
    public Renderer[] wire2;
    public Renderer[] wire3;
    public Renderer[] wire4;
    public Renderer[] buttons;
    public Renderer exeButton;
    private int wireIndex = 0;
    private List<int> selectedWireTextures = new List<int>();

    //Buttons
    private int buttonColourIndex = 0;
    public TextMesh[] buttonLabels;
    public TextMesh exeButtonLabel;
    private int buttonLabelIndex = 0;
    public string[] letterOptions;
    private List<int> selectedButtonColoursIndex = new List<int>();
    private List<int> selectedButtonLabelsIndex = new List<int>();
    private int buttonDistance = 0;
    private bool generalButtonLock = false;
    int executeRotation = 0;
    private bool executeLock = false;
    private List<Renderer> pushedButtons = new List<Renderer>();
    public bool[] localButtonLock;

    //Screens
    public TextMesh screen1Display;
    public TextMesh screen2Display;
    private string screen1String = "";
    private string screen2String = "";
    private int screenLetterIndex = 0;
    private int nextLetter1 = 0;
    private int nextLetter2 = 0;

    //Codes etc.
    private List<int> rotationCodes = new List<int>();
    private Texture wire3Colour;
    private Texture wire1Colour;
    private int wire3ColourNumber;
    private int wire1ColourNumber;
    private List<int> wireCodes = new List<int>();
    private int blueButtons = 0;
    private int greenButtons = 0;

    //Ciphers
    private bool ciphersLogged = false;
    private List<int> cipher1Digits = new List<int>();
    private List<int> cipher2Digits = new List<int>();
    private List<int> cipher3Digits = new List<int>();
    private List<int> finalCipher = new List<int>();

    //Correct Answers
    private int stage = 1;
    public bool[] correctButtons;
    public bool[] buttonPushed;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;

    //TP
    bool solveCoroStarted = false;
    bool pressingButtons = false;
    bool turnCommand = false;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += OnActivate;
        foreach (KMSelectable button in numberButtons)
        {
            KMSelectable trueButton = button;
            button.OnInteract += delegate () { numberButtonPress(trueButton); return false; };
        }
        executeButton.OnInteract += delegate () { executeButtonPress(); return false; };
    }

    void Start()
    {
        while (selectedRotations.Count != 6)
        {
            int selection = UnityEngine.Random.Range(0, 6);

            // Prevent the rare case of having only clockwise and counter-clockwise rotations (0 and 3) because that would make the bottom face near-impossible to see.
            if (selectedRotations.Count == 5 && selectedRotations.All(s => s == 0 || s == 3))
            {
                selection = UnityEngine.Random.Range(1, 5);
                if (selection >= 3)
                    selection++;
            }

            selectedRotations.Add(selection);
        }

        Debug.LogFormat("[The Cube #{0}] Cube movements: #1 is {1}. #2 is {2}. #3 is {3}. #4 is {4}. #5 is {5}. #6 is {6}.", moduleId, rotationName[selectedRotations[0]], rotationName[selectedRotations[1]], rotationName[selectedRotations[2]], rotationName[selectedRotations[3]], rotationName[selectedRotations[4]], rotationName[selectedRotations[5]]);
        foreach (TextMesh digit in cubeScreens)
        {
            int screenText = UnityEngine.Random.Range(0, 10);
            cubeNumbers.Add(screenText);
            digit.text = screenText.ToString();
        }
        Debug.LogFormat("[The Cube #{0}] Cube faces: #1 is {1}. #2 is {2}. #3 is {3}. #4 is {4}. #5 is {5}. #6 is {6}.", moduleId, cubeNumbers[0], cubeNumbers[1], cubeNumbers[2], cubeNumbers[3], cubeNumbers[4], cubeNumbers[5]);
        buttonPicker();
        wirePicker();
        screenWords();
    }

    void OnActivate()
    {
        StartCoroutine(cubeRotation());
        StartCoroutine(stageCounter());
    }
    void buttonPicker()
    {
        foreach (Renderer button in buttons)
        {
            buttonColourIndex = UnityEngine.Random.Range(0, 6);
            selectedButtonColoursIndex.Add(buttonColourIndex);
            button.material.mainTexture = colourOptions[buttonColourIndex];
        }

        buttonColourIndex = UnityEngine.Random.Range(0, 6);
        selectedButtonColoursIndex.Add(buttonColourIndex);
        exeButton.material.mainTexture = colourOptions[buttonColourIndex];

        foreach (TextMesh buttonLabel in buttonLabels)
        {
            buttonLabelIndex = UnityEngine.Random.Range(0,17);
            selectedButtonLabelsIndex.Add(buttonLabelIndex);
            buttonLabel.text = letterOptions[buttonLabelIndex];
        }

        buttonLabelIndex = UnityEngine.Random.Range(0,17);
        selectedButtonLabelsIndex.Add(buttonLabelIndex);
        exeButtonLabel.text = letterOptions[buttonLabelIndex];

        Debug.LogFormat("[The Cube #{0}] Buttons: #1 is {1} and says {2}. #2 is {3} and says {4}. #3 is {5} and says {6}. #4 is {7} and says {8}. #5 is {9} and says {10}. #6 is {11} and says {12}. #7 is {13} and says {14}. #8 is {15} and says {16}. The execute button is {17} and says {18}.", moduleId, colourOptions[selectedButtonColoursIndex[0]].name.Replace("Mat", ""), letterOptions[selectedButtonLabelsIndex[0]], colourOptions[selectedButtonColoursIndex[1]].name.Replace("Mat", ""), letterOptions[selectedButtonLabelsIndex[1]], colourOptions[selectedButtonColoursIndex[2]].name.Replace("Mat", ""), letterOptions[selectedButtonLabelsIndex[2]], colourOptions[selectedButtonColoursIndex[3]].name.Replace("Mat", ""), letterOptions[selectedButtonLabelsIndex[3]], colourOptions[selectedButtonColoursIndex[4]].name.Replace("Mat", ""), letterOptions[selectedButtonLabelsIndex[4]], colourOptions[selectedButtonColoursIndex[5]].name.Replace("Mat", ""), letterOptions[selectedButtonLabelsIndex[5]], colourOptions[selectedButtonColoursIndex[6]].name.Replace("Mat", ""), letterOptions[selectedButtonLabelsIndex[6]], colourOptions[selectedButtonColoursIndex[7]].name.Replace("Mat", ""), letterOptions[selectedButtonLabelsIndex[7]], colourOptions[selectedButtonColoursIndex[8]].name.Replace("Mat", ""), letterOptions[selectedButtonLabelsIndex[8]]);

        blueButtons = buttons.Count((x) => x.material.mainTexture.name == "blueMat");
        greenButtons = buttons.Count((x) => x.material.mainTexture.name == "greenMat");
    }

    void wirePicker()
    {
        wireIndex = UnityEngine.Random.Range(0, 6);
        selectedWireTextures.Add(wireIndex);
        foreach (Renderer wire in wire1)
        {
            wire.material.mainTexture = colourOptions[wireIndex];
        }
        if (wire1[0].material.mainTexture == colourOptions[0])
        {
            wireCodes.Add(6);
        }
        else if (wire1[0].material.mainTexture == colourOptions[1])
        {
            wireCodes.Add((blueButtons + 7) % 10);
        }
        else if (wire1[0].material.mainTexture == colourOptions[2])
        {
            wireCodes.Add((greenButtons + 3) % 10);
        }
        else if (wire1[0].material.mainTexture == colourOptions[3])
        {
            wireCodes.Add((cubeNumbers[0] + cubeNumbers[1] + cubeNumbers[2] + cubeNumbers[3] + cubeNumbers[4] + cubeNumbers[5]) % 10);
        }
        else if (wire1[0].material.mainTexture == colourOptions[4])
        {
            wireCodes.Add((Bomb.GetModuleNames().Count() + 7) % 10);
        }
        else if (wire1[0].material.mainTexture == colourOptions[5])
        {
            wireCodes.Add(6);
        }

        wireIndex = UnityEngine.Random.Range(0, 6);
        selectedWireTextures.Add(wireIndex);
        foreach (Renderer wire in wire2)
        {
            wire.material.mainTexture = colourOptions[wireIndex];
        }
        if (wire2[0].material.mainTexture == colourOptions[0])
        {
            wireCodes.Add(7);
        }
        else if (wire2[0].material.mainTexture == colourOptions[1])
        {
            wireCodes.Add((blueButtons + 7) % 10);
        }
        else if (wire2[0].material.mainTexture == colourOptions[2])
        {
            wireCodes.Add((greenButtons + 3) % 10);
        }
        else if (wire2[0].material.mainTexture == colourOptions[3])
        {
            wireCodes.Add((cubeNumbers[0] + cubeNumbers[1] + cubeNumbers[2] + cubeNumbers[3] + cubeNumbers[4] + cubeNumbers[5]) % 10);
        }
        else if (wire2[0].material.mainTexture == colourOptions[4])
        {
            wireCodes.Add((Bomb.GetModuleNames().Count() + 7) % 10);
        }
        else if (wire2[0].material.mainTexture == colourOptions[5])
        {
            wireCodes.Add(6);
        }

        wireIndex = UnityEngine.Random.Range(0, 6);
        selectedWireTextures.Add(wireIndex);
        foreach (Renderer wire in wire3)
        {
            wire.material.mainTexture = colourOptions[wireIndex];
        }
        if (wire3[0].material.mainTexture == colourOptions[0])
        {
            wireCodes.Add(8);
        }
        else if (wire3[0].material.mainTexture == colourOptions[1])
        {
            wireCodes.Add((blueButtons + 7) % 10);
        }
        else if (wire3[0].material.mainTexture == colourOptions[2])
        {
            wireCodes.Add((greenButtons + 3) % 10);
        }
        else if (wire3[0].material.mainTexture == colourOptions[3])
        {
            wireCodes.Add((cubeNumbers[0] + cubeNumbers[1] + cubeNumbers[2] + cubeNumbers[3] + cubeNumbers[4] + cubeNumbers[5]) % 10);
        }
        else if (wire3[0].material.mainTexture == colourOptions[4])
        {
            wireCodes.Add((Bomb.GetModuleNames().Count() + 7) % 10);
        }
        else if (wire3[0].material.mainTexture == colourOptions[5])
        {
            wireCodes.Add(6);
        }

        wireIndex = UnityEngine.Random.Range(0, 6);
        selectedWireTextures.Add(wireIndex);
        foreach (Renderer wire in wire4)
        {
            wire.material.mainTexture = colourOptions[wireIndex];
        }
        if (wire4[0].material.mainTexture == colourOptions[0])
        {
            wireCodes.Add(9);
        }
        else if (wire4[0].material.mainTexture == colourOptions[1])
        {
            wireCodes.Add((blueButtons + 7) % 10);
        }
        else if (wire4[0].material.mainTexture == colourOptions[2])
        {
            wireCodes.Add((greenButtons + 3) % 10);
        }
        else if (wire4[0].material.mainTexture == colourOptions[3])
        {
            wireCodes.Add((cubeNumbers[0] + cubeNumbers[1] + cubeNumbers[2] + cubeNumbers[3] + cubeNumbers[4] + cubeNumbers[5]) % 10);
        }
        else if (wire4[0].material.mainTexture == colourOptions[4])
        {
            wireCodes.Add((Bomb.GetModuleNames().Count() + 7) % 10);
        }
        else if (wire4[0].material.mainTexture == colourOptions[5])
        {
            wireCodes.Add(6);
        }
        Debug.LogFormat("[The Cube #{0}] Wires: #1 is {1} #2 is {2} #3 is {3} #4 is {4}", moduleId, colourOptions[selectedWireTextures[0]].name.Replace("Mat", "."), colourOptions[selectedWireTextures[1]].name.Replace("Mat", "."), colourOptions[selectedWireTextures[2]].name.Replace("Mat", "."), colourOptions[selectedWireTextures[3]].name.Replace("Mat", "."));
    }

    void screenWords()
    {
        screen1Display.text = screen1String;
        screen2Display.text = screen2String;
        while (screen1String.Count() < 8)
        {
            screenLetterIndex = UnityEngine.Random.Range(0, 17);
            screen1String += letterOptions[screenLetterIndex];
        }
        while (screen2String.Count() < 8)
        {
            screenLetterIndex = UnityEngine.Random.Range(0, 17);
            screen2String += letterOptions[screenLetterIndex];
        }
        Debug.LogFormat("[The Cube #{0}] Screens: #1 says {1}. #2 says {2}.", moduleId, screen1String, screen2String);
        cipher2Digits = screen1String.Select(ch => (ch - 'A' + 1) % 10).ToList();
        cipher3Digits = screen2String.Select(ch => (ch - 'A' + 1) % 10).ToList();

        StartCoroutine(textAnimation1());
        StartCoroutine(textAnimation2());
    }

    private IEnumerator textAnimation1()
    {
        while (moduleSolved == false)
        {
            while (screen1Display.text.Count() < 8)
            {
                yield return new WaitForSeconds(1.2f);
                screen1Display.text += screen1String[0 + nextLetter1];
                nextLetter1++;

                if (screen1Display.text.Count() == 8)
                {
                    yield return new WaitForSeconds(2.4f);
                    if (moduleSolved)
                    {

                    }
                    else
                    {
                        screen1Display.text = "";
                        nextLetter1 = 0;
                    }
                }
            }
        }
    }

    private IEnumerator textAnimation2()
    {
        while (moduleSolved == false)
        {
            while (screen2Display.text.Count() < 8)
            {
                yield return new WaitForSeconds(0.7f);
                screen2Display.text += screen2String[0 + nextLetter2];
                nextLetter2++;

                if (screen2Display.text.Count() == 8)
                {
                    yield return new WaitForSeconds(2.4f);
                    if (moduleSolved)
                    {

                    }
                    else
                    {
                        screen2Display.text = "";
                        nextLetter2 = 0;
                    }
                }
            }
        }
    }

    private IEnumerator cubeRotation()
    {
        while (moduleSolved == false)
        {
            if (selectionIncreaser == 6)
            {
                selectionIncreaser = 0;
            }
            axisSelection = selectedRotations[0 + selectionIncreaser];

            if (axisSelection == 0)
            {
                while (rotation != 90)
                {
                    yield return new WaitForSeconds(0.02f);
                    cipherBox.transform.localRotation = Quaternion.Euler(0.0f, 1.0f, 0.0f) * cipherBox.transform.localRotation;;
                    rotation++;
                }
            rotationCodes.Add(4);
            yield return new WaitForSeconds(0.1f);
            rotation = 0;
            }

            else if (axisSelection == 1)
            {
                while (rotation != 90)
                {
                    yield return new WaitForSeconds(0.02f);
                    cipherBox.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 1.0f) * cipherBox.transform.localRotation;;
                    rotation++;
                }
                wire3Colour = wire3[0].material.mainTexture;
                foreach (Renderer button in buttons)
                {
                    if (button.material.mainTexture == wire3Colour)
                    {
                        wire3ColourNumber++;
                    }
                }
                rotationCodes.Add(wire3ColourNumber);
                wire3ColourNumber = 0;
                yield return new WaitForSeconds(0.1f);
                rotation = 0;
            }

            else if (axisSelection == 2)
            {
                while (rotation != 90)
                {
                    yield return new WaitForSeconds(0.02f);
                    cipherBox.transform.localRotation = Quaternion.Euler(1.0f, 0.0f, 0.0f) * cipherBox.transform.localRotation;;
                    rotation++;
                }
            rotationCodes.Add(Bomb.GetSerialNumberNumbers().Last());
            yield return new WaitForSeconds(0.1f);
            rotation = 0;
            }

            else if (axisSelection == 3)
            {
                while (rotation != 90)
                {
                    yield return new WaitForSeconds(0.02f);
                    cipherBox.transform.localRotation = Quaternion.Euler(0.0f, -1.0f, 0.0f) * cipherBox.transform.localRotation;;
                    rotation++;
                }
            rotationCodes.Add(7);
            yield return new WaitForSeconds(0.1f);
            rotation = 0;
            }

            else if (axisSelection == 4)
            {
                while (rotation != 90)
                {
                    yield return new WaitForSeconds(0.02f);
                    cipherBox.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -1.0f) * cipherBox.transform.localRotation;;
                    rotation++;
                }
                wire1Colour = wire1[0].material.mainTexture;
                foreach (Renderer button in buttons)
                {
                    if (button.material.mainTexture == wire1Colour)
                    {
                        wire1ColourNumber++;
                    }
                }
            rotationCodes.Add(wire1ColourNumber);
            wire1ColourNumber = 0;
            yield return new WaitForSeconds(0.1f);
            rotation = 0;
            }

            else if (axisSelection == 5)
            {
                while (rotation != 90)
                {
                    yield return new WaitForSeconds(0.02f);
                    cipherBox.transform.localRotation = Quaternion.Euler(-1.0f, 0.0f, 0.0f) * cipherBox.transform.localRotation;;
                    rotation++;
                }
                rotationCodes.Add(Bomb.GetSerialNumberNumbers().First());
            }

            if (selectionIncreaser == 5)
            {
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
            rotation = 0;
            while (turnCommand)
            {
                yield return new WaitForSeconds(2f);
            }
            selectionIncreaser++;
            if (selectionIncreaser == 6 && ciphersLogged == false)
            {
                rotationComplete = true;
                //Audio rotation complete
                ciphersLogged = true;
                cipherLogging();
                answerCalculator();
            }
        }
    }

    private IEnumerator stageCounter()
    {
        while (moduleSolved == false)
        {
            yield return new WaitForSeconds(0.04f);
            stageWheel.transform.localRotation = Quaternion.Euler(0.0f, 1.0f, 0.0f) * stageWheel.transform.localRotation;;
            logoWheel.transform.localRotation = Quaternion.Euler(0.0f, -1.0f, 0.0f) * logoWheel.transform.localRotation;;
        }
    }

    void cipherLogging()
    {
        cipher1Digits.Add((rotationCodes[0] + cubeNumbers[5] + wireCodes[2]) % 10);
        cipher1Digits.Add((rotationCodes[1] + cubeNumbers [4] + wireCodes[3]) % 10);
        cipher1Digits.Add((rotationCodes[2] + cubeNumbers [3] + wireCodes[0]) % 10);
        cipher1Digits.Add((rotationCodes[3] + cubeNumbers [2] + wireCodes[1]) % 10);
        cipher1Digits.Add((rotationCodes[4] + cubeNumbers [1]) % 8);
        cipher1Digits.Add((rotationCodes[5] + cubeNumbers [0]) % 9);
        Debug.LogFormat("[The Cube #{0}] The rotation codes are {1}, {2}, {3}, {4}, {5}, {6}.", moduleId, rotationCodes[0], rotationCodes[1], rotationCodes[2], rotationCodes[3], rotationCodes[4], rotationCodes[5]);
        Debug.LogFormat("[The Cube #{0}] The wire codes are {1}, {2}, {3}, {4}.", moduleId, wireCodes[0], wireCodes[1], wireCodes[2], wireCodes[3]);
        Debug.LogFormat("[The Cube #{0}] Cipher 1 is {1}{2}{3}{4}{5}{6}.", moduleId, cipher1Digits[0], cipher1Digits[1], cipher1Digits[2], cipher1Digits[3], cipher1Digits[4], cipher1Digits[5]);
        Debug.LogFormat("[The Cube #{0}] Cipher 2 is {1}{2}{3}{4}{5}{6}{7}{8}.", moduleId, cipher2Digits[0], cipher2Digits[1], cipher2Digits[2], cipher2Digits[3], cipher2Digits[4], cipher2Digits[5], cipher2Digits[6], cipher2Digits[7]);
        Debug.LogFormat("[The Cube #{0}] Cipher 3 is {1}{2}{3}{4}{5}{6}{7}{8}.", moduleId, cipher3Digits[0], cipher3Digits[1], cipher3Digits[2], cipher3Digits[3], cipher3Digits[4], cipher3Digits[5], cipher3Digits[6], cipher3Digits[7]);
        finalCipher.Add((cipher1Digits[0] + cipher2Digits[0] + cipher3Digits[0]) % 10);
        finalCipher.Add((cipher1Digits[1] + cipher2Digits[1] + cipher3Digits[1]) % 10);
        finalCipher.Add((cipher1Digits[2] + cipher2Digits[2] + cipher3Digits[2]) % 10);
        finalCipher.Add((cipher1Digits[3] + cipher2Digits[3] + cipher3Digits[3]) % 10);
        finalCipher.Add((cipher1Digits[4] + cipher2Digits[4] + cipher3Digits[4]) % 10);
        finalCipher.Add((cipher1Digits[5] + cipher2Digits[5] + cipher3Digits[5]) % 10);
        finalCipher.Add((cipher2Digits[6] + cipher3Digits[6]) % 10);
        finalCipher.Add((cipher2Digits[7] + cipher3Digits[7]) % 10);
        Debug.LogFormat("[The Cube #{0}] The final cipher is {1}{2}{3}{4}{5}{6}{7}{8}.", moduleId, finalCipher[0], finalCipher[1], finalCipher[2], finalCipher[3], finalCipher[4], finalCipher[5], finalCipher[6], finalCipher[7]);
    }

    public void answerCalculator()
    {
        var digit = finalCipher[stage - 1];
        if (digit == 0)
        {
            for (int labelIndex = 0; labelIndex < buttonLabels.Length; ++labelIndex)
            {
                TextMesh label = buttonLabels[labelIndex];
                if (label.text == "A" || label.text == "F" || label.text == "I" || label.text == "L")
                {
                    correctButtons[labelIndex] = true;
                }
            }
        }
        else if (digit == 1)
        {
            for (int labelIndex = 0; labelIndex < buttonLabels.Length; ++labelIndex)
            {
                TextMesh label = buttonLabels[labelIndex];
                if (label.text == "B" || label.text == "E" || label.text == "K" || label.text == "O")
                {
                    correctButtons[labelIndex] = true;
                }
            }
        }
        else if (digit == 2)
        {
            for (int labelIndex = 0; labelIndex < buttonLabels.Length; ++labelIndex)
            {
                TextMesh label = buttonLabels[labelIndex];
                if (label.text == "D" || label.text == "N" || label.text == "Q")
                {
                    correctButtons[labelIndex] = true;
                }
            }
        }
        else if (digit == 3)
        {
            for (int labelIndex = 0; labelIndex < buttonLabels.Length; ++labelIndex)
            {
                TextMesh label = buttonLabels[labelIndex];
                if (label.text == "C" || label.text == "G" || label.text == "P")
                {
                    correctButtons[labelIndex] = true;
                }
            }
        }
        else if (digit == 4)
        {
            for (int labelIndex = 0; labelIndex < buttonLabels.Length; ++labelIndex)
            {
                TextMesh label = buttonLabels[labelIndex];
                if (label.text == "H" || label.text == "J" || label.text == "M")
                {
                    correctButtons[labelIndex] = true;
                }
            }
        }
        else if (digit == 5)
        {
            for (int labelIndex = 0; labelIndex < buttonLabels.Length; ++labelIndex)
            {
                TextMesh label = buttonLabels[labelIndex];
                if (label.text == "E" || label.text == "J" || label.text == "Q")
                {
                    correctButtons[labelIndex] = true;
                }
            }
        }
        else if (digit == 6)
        {
            for (int labelIndex = 0; labelIndex < buttonLabels.Length; ++labelIndex)
            {
                TextMesh label = buttonLabels[labelIndex];
                if (label.text == "F" || label.text == "L" || label.text == "P")
                {
                    correctButtons[labelIndex] = true;
                }
            }
        }
        else if (digit == 7)
        {
            for (int labelIndex = 0; labelIndex < buttonLabels.Length; ++labelIndex)
            {
                TextMesh label = buttonLabels[labelIndex];
                if (label.text == "A" || label.text == "K" || label.text == "M")
                {
                    correctButtons[labelIndex] = true;
                }
            }
        }
        else if (digit == 8)
        {
            for (int labelIndex = 0; labelIndex < buttonLabels.Length; ++labelIndex)
            {
                TextMesh label = buttonLabels[labelIndex];
                if (label.text == "C" || label.text == "G" || label.text == "H" || label.text == "O")
                {
                    correctButtons[labelIndex] = true;
                }
            }
        }
        else if (digit == 9)
        {
            for (int labelIndex = 0; labelIndex < buttonLabels.Length; ++labelIndex)
            {
                TextMesh label = buttonLabels[labelIndex];
                if (label.text == "B" || label.text == "D" || label.text == "I" || label.text == "N")
                {
                    correctButtons[labelIndex] = true;
                }
            }
        }
        if (stage == 2)
        {
            for (int labelIndex = 0; labelIndex < buttonLabels.Length; ++labelIndex)
            {
                TextMesh label = buttonLabels[labelIndex];
                {
                    if (label.text == exeButtonLabel.text)
                    {
                        correctButtons[labelIndex] = true;
                    }
                }
            }
        }
        else if (stage == 4)
        {
            for (int labelIndex = 0; labelIndex < buttons.Length; ++labelIndex)
            {
                Renderer button = buttons[labelIndex];
                {
                    if (button.material.mainTexture == exeButton.material.mainTexture)
                    {
                        correctButtons[labelIndex] = true;
                    }
                }
            }
        }
        else if (stage == 6)
        {
            for (int labelIndex = 0; labelIndex < buttons.Length; ++labelIndex)
            {
                Renderer button = buttons[labelIndex];
                {
                    if (button.material.mainTexture == wire1[0].material.mainTexture)
                    {
                        correctButtons[labelIndex] = true;
                    }
                }
            }
        }
        else if (stage == 7)
        {
            for (int labelIndex = 0; labelIndex < buttons.Length; ++labelIndex)
            {
                Renderer button = buttons[labelIndex];
                {
                    if (button.material.mainTexture == wire3[0].material.mainTexture)
                    {
                        correctButtons[labelIndex] = true;
                    }
                }
            }
        }
        else if (stage == 8)
        {
            for (int labelIndex = 0; labelIndex < buttons.Length; ++labelIndex)
            {
                if (correctButtons[labelIndex])
                {
                    correctButtons[labelIndex] = false;
                }
                else
                {
                    correctButtons[labelIndex] = true;
                }
            }
        }
        Debug.LogFormat("[The Cube #{0}] Stage {1} button presses: #1 is {2}. #2 is {3}. #3 is {4}. #4 is {5}. #5 is {6}. #6 is {7}. #7 is {8}. #8 is {9}.", moduleId, stage, correctButtons[0], correctButtons[1], correctButtons[2], correctButtons[3], correctButtons[4], correctButtons[5], correctButtons[6], correctButtons[7]);
    }

    public void numberButtonPress(KMSelectable button)
    {
        int pressedButton = Array.IndexOf(numberButtons, button);
        if (generalButtonLock || executeLock || localButtonLock[pressedButton])
        {

        }
        else
        {
            beepIndex = UnityEngine.Random.Range(0,10);
            Audio.PlaySoundAtTransform(beeps[beepIndex].name, transform);
            button.AddInteractionPunch(.5f);
            generalButtonLock = true;
            executeLock = true;
            localButtonLock[pressedButton] = true;
            buttonPushed[pressedButton] = true;
            pushedButtons.Add(buttons[pressedButton]);
            StartCoroutine(buttonAnimation(pressedButton));
        }
    }

    private IEnumerator buttonAnimation(int pressedButton)
    {
        while (buttonDistance < 8)
        {
            yield return new WaitForSeconds(0.003f);
            numberButtons[pressedButton].transform.localPosition = numberButtons[pressedButton].transform.localPosition + Vector3.up * -0.001f;
            buttonDistance ++;
        }
        if (buttonDistance == 8)
        {
            buttonDistance = 0;
            generalButtonLock = false;
            executeLock = false;
        }
    }

    private IEnumerator buttonAnimationUndo()
    {
        generalButtonLock = true;
        executeLock = true;
        while (executeRotation != 36)
        {
            yield return new WaitForSeconds(0.02f);
            executeButton.transform.localRotation = Quaternion.Euler(0.0f, -10.0f, 0.0f) * executeButton.transform.localRotation;;
            executeButton.transform.localPosition = executeButton.transform.localPosition + Vector3.down * -0.0003f;
            executeRotation++;
        }
        executeRotation = 0;;
        foreach (Renderer button in pushedButtons)
        {
            while (buttonDistance < 8)
            {
                yield return new WaitForSeconds(0.003f);
                button.transform.localPosition = button.transform.localPosition + Vector3.down * -0.001f;
                buttonDistance ++;
            }
            if (buttonDistance == 8)
            {
                buttonDistance = 0;
            }
        }
        pushedButtons.Clear();
        for (int lockIndex = 0; lockIndex < localButtonLock.Length; ++lockIndex)
        {
            localButtonLock[lockIndex] = false;
        }
        for (int lockIndex = 0; lockIndex < buttonPushed.Length; ++lockIndex)
        {
            buttonPushed[lockIndex] = false;
        }
        if (moduleSolved)
        {

        }
        else
        {
            generalButtonLock = false;
            executeLock = false;
        }
    }

    public void executeButtonPress()
    {
        if (generalButtonLock || executeLock || rotationComplete == false)
        {

        }
        else
        {
            beepIndex = UnityEngine.Random.Range(0,10);
            Audio.PlaySoundAtTransform("dial", transform);
            executeButton.AddInteractionPunch(.5f);
            generalButtonLock = true;
            executeLock = true;
            StartCoroutine(executeAnimation());
        }
    }

    private IEnumerator executeAnimation()
    {
        while (executeRotation != 36)
        {
            yield return new WaitForSeconds(0.02f);
            executeButton.transform.localRotation = Quaternion.Euler(0.0f, 10.0f, 0.0f) * executeButton.transform.localRotation;;
            executeButton.transform.localPosition = executeButton.transform.localPosition + Vector3.up * -0.0003f;
            executeRotation++;
        }

        executeRotation = 0;;
        Audio.PlaySoundAtTransform("calculation", transform);
        yield return new WaitForSeconds(5f);
        if (buttonPushed[0] == correctButtons[0] && buttonPushed[1] == correctButtons[1] && buttonPushed[2] == correctButtons[2] && buttonPushed[3] == correctButtons[3] && buttonPushed[4] == correctButtons[4] && buttonPushed[5] == correctButtons[5] && buttonPushed[6] == correctButtons[6] && buttonPushed[7] == correctButtons[7])
        {
            switch (stage)
            {
                case 1:
                Debug.LogFormat("[The Cube #{0}] You pressed the correct buttons. Stage 1 passed.", moduleId);
                Audio.PlaySoundAtTransform("success", transform);
                StartCoroutine(buttonAnimationUndo());
                lights[0].material.mainTexture = lightColours[1];
                stage++;
                for (int index = 0; index < correctButtons.Length; ++index)
                {
                    correctButtons[index] = false;
                }
                answerCalculator();
                break;

                case 2:
                Debug.LogFormat("[The Cube #{0}] You pressed the correct buttons. Stage 2 passed.", moduleId);
                Audio.PlaySoundAtTransform("success", transform);
                StartCoroutine(buttonAnimationUndo());
                lights[3].material.mainTexture = lightColours[1];
                stage++;
                for (int index = 0; index < correctButtons.Length; ++index)
                {
                    correctButtons[index] = false;
                }
                answerCalculator();
                break;

                case 3:
                Debug.LogFormat("[The Cube #{0}] You pressed the correct buttons. Stage 3 passed.", moduleId);
                Audio.PlaySoundAtTransform("success", transform);
                StartCoroutine(buttonAnimationUndo());
                lights[6].material.mainTexture = lightColours[1];
                stage++;
                for (int index = 0; index < correctButtons.Length; ++index)
                {
                    correctButtons[index] = false;
                }
                answerCalculator();
                break;

                case 4:
                Debug.LogFormat("[The Cube #{0}] You pressed the correct buttons. Stage 4 passed.", moduleId);
                Audio.PlaySoundAtTransform("success", transform);
                StartCoroutine(buttonAnimationUndo());
                lights[2].material.mainTexture = lightColours[1];
                stage++;
                for (int index = 0; index < correctButtons.Length; ++index)
                {
                    correctButtons[index] = false;
                }
                answerCalculator();
                break;

                case 5:
                Debug.LogFormat("[The Cube #{0}] You pressed the correct buttons. Stage 5 passed.", moduleId);
                Audio.PlaySoundAtTransform("success", transform);
                StartCoroutine(buttonAnimationUndo());
                lights[7].material.mainTexture = lightColours[1];
                stage++;
                for (int index = 0; index < correctButtons.Length; ++index)
                {
                    correctButtons[index] = false;
                }
                answerCalculator();
                break;

                case 6:
                Debug.LogFormat("[The Cube #{0}] You pressed the correct buttons. Stage 6 passed.", moduleId);
                Audio.PlaySoundAtTransform("success", transform);
                StartCoroutine(buttonAnimationUndo());
                lights[4].material.mainTexture = lightColours[1];
                stage++;
                for (int index = 0; index < correctButtons.Length; ++index)
                {
                    correctButtons[index] = false;
                }
                answerCalculator();
                break;

                case 7:
                Debug.LogFormat("[The Cube #{0}] You pressed the correct buttons. Stage 7 passed.", moduleId);
                Audio.PlaySoundAtTransform("success", transform);
                StartCoroutine(buttonAnimationUndo());
                lights[1].material.mainTexture = lightColours[1];
                stage++;
                for (int index = 0; index < correctButtons.Length; ++index)
                {
                    correctButtons[index] = false;
                }
                answerCalculator();
                break;

                case 8:
                solveCoroStarted = true;
                Debug.LogFormat("[The Cube #{0}] You pressed the correct buttons. Stage 8 passed. Mothership contacted. Module disarmed.", moduleId);
                Audio.PlaySoundAtTransform("contact", transform);
                StartCoroutine(buttonAnimationUndo());
                lights[5].material.mainTexture = lightColours[1];
                stage++;
                for (int index = 0; index < correctButtons.Length; ++index)
                {
                    correctButtons[index] = false;
                }
                GetComponent<KMBombModule>().HandlePass();
                generalButtonLock = true;
                executeLock = true;
                moduleSolved = true;
                break;

                default:
                break;
            }
        }
        else
        {
            Debug.LogFormat("[The Cube #{0}] Strike! At stage {1}, your buttons were: #1 is {2}. #2 is {3}. #3 is {4}. #4 is {5}. #5 is {6}. #6 is {7}. #7 is {8}. #8 is {9}. That is incorrect. The module has been reset to stage 1.", moduleId, stage, buttonPushed[0], buttonPushed[1], buttonPushed[2], buttonPushed[3], buttonPushed[4], buttonPushed[5], buttonPushed[6], buttonPushed[7]);
            GetComponent<KMBombModule>().HandleStrike();
            StartCoroutine(buttonAnimationUndo());
            foreach (Renderer light in lights)
            {
                light.material.mainTexture = lightColours[0];
            }
            stage = 1;
            for (int index = 0; index < correctButtons.Length; ++index)
            {
                correctButtons[index] = false;
            }
            answerCalculator();
        }
        if (moduleSolved == false)
        {
            executeLock = false;
        }
    }

#pragma warning disable 414
    private string TwitchHelpMessage = @"Press the buttons in reading order with !{0} press 1 2 3. Press the execute button with !{0} execute. Look at the faces of the cube with !{0} turn.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        var parts = command.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1 && parts[0] == "turn" && !turnCommand)
        {
            yield return null;
            turnCommand = true;
            yield return new WaitForSeconds(4f);
            while (rotation != 360)
            {
                yield return new WaitForSeconds(0.02f);
                cipherBox.transform.localRotation = Quaternion.Euler(1.0f, 0.0f, 0.0f) * cipherBox.transform.localRotation; ;
                rotation++;
            }
            rotation = 0;
            while (rotation != 360)
            {
                yield return new WaitForSeconds(0.02f);
                cipherBox.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 1.0f) * cipherBox.transform.localRotation; ;
                rotation++;
            }
            rotation = 0;
            yield return new WaitForSeconds(4f);
            turnCommand = false;
            yield break;
        }

        if (parts.Length == 1 && parts[0] == "execute")
        {
            while (pressingButtons)
            {
                yield return new WaitForSeconds(1f);
            }
            yield return null;
            executeButtonPress();
            yield break;
        }

        if (parts.Length > 1 && parts[0] == "press" && parts.Skip(1).All(part => part.Length == 1 && "12345678".Contains(part)))
        {
            yield return null;

            pressingButtons = true;
            var cmdNumbers = parts.Skip(1).ToArray();

            for ( int i = 0; i < cmdNumbers.Length; i++)
            {
                int num;
                int.TryParse(cmdNumbers[i], out num);

                numberButtonPress(numberButtons[num - 1]);
                yield return new WaitForSeconds(.3f);
            }
            pressingButtons = false;
        }

        if (solveCoroStarted)
        {
            yield return "solve";
        }
    }
}
