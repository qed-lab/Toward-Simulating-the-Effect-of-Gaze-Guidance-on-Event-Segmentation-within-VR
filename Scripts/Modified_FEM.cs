// -----------------------------------------------------------------------------
// DISCLAIMER:
// This code was originally converted from Visual Basic to C# and modified
// with additional functionality (a new factor termed "AssessCueInfluence")
// to support experimental research purposes. The original code is by Gabriel
// Radvansky and is available here:
// doi:10.7274/R08913SH
// The original logic, structure, and intent of the source code may have been
// adapted or extended to fit specific experimental requirements, including the
// Use, modification, or distribution of this code should be done with proper
// attribution and at your own risk.
//
// Author: [Monthir Ali]
// Date: [12/2024]
// -----------------------------------------------------------------------------


using System;
using System.IO;

public class FEM {

    public int EndFlag;
    static public int[] Subject = new int[20400];
    static public int[] Condition = new int[20400];
    static public int[] Score = new int[20400];
    static public int[] Trial = new int[20400];
    static public double[] Composite = new double[20400];
    static public double[] CueType = new double[20400];
    static public double[] GazeProximity = new double[20400];
    static public int PreviousSubject;
    static public int[] Angle = new int[20400];
    static public int AngleDiff;
    static public int[] Morph = new int[20400];
    static public int SubjectTrial;
    static public int iterator = 0;
    static public int RunSize;
    static public string[] Strategy = new string[20400];
    static public string PreviousStrategy;
    static public string[] Expected = new string[20400];

    //Used in Subsections
    static public double Prediction;
    static public double TaskShift;
    static public double PreviousCondition;
    static public double AlreadyDoing;
    static public double JustShifted;
    static public double TaskTrial;
    static public double CueInfluence;
    static public int StrategySwitch;
    static public int LastSwitch;
    static public double PreviousTaskTrial;
    static public int NumStrategies;
    static public int NumComponents;
    static public double MinTime;
    static public double MaxTime;
    static public double[] RecentTimes = new double[11];
    static public double PlanningPrep;
    static public double HowFarOut;
    static public double StrategiesTried;
    static public double[] Alpha = new double[10];
    static public double[] RecentPerformance = new double[11];
    static public double BadShift;
    static public double MinPerformance;
    static public double MaxPerformance;
    static public double HowBadIsIt;
    static public double PerformanceDip;
    static public double slope;
    static public double NoImprovement;
    static public int WM;
    static public double Flexibility;

    static public StreamWriter outFile = new StreamWriter(@"C:\Users\anonymous\OneDrive\Desktop\SF2023\FEM_Code\data\QJEP paper data\Experiment 2\output_NoDifference_override.csv");

    static void Main() {
        // Load the data
        StreamReader[] files = {
            new StreamReader(@"C:\Users\anonymous\OneDrive\Desktop\SF2023\FEM_Code\data\QJEP paper data\Experiment 2\Subject.txt"),
            new StreamReader(@"C:\Users\anonymous\OneDrive\Desktop\SF2023\FEM_Code\data\QJEP paper data\Experiment 2\Condition.txt"),
            new StreamReader(@"C:\Users\anonymous\OneDrive\Desktop\SF2023\FEM_Code\data\QJEP paper data\Experiment 2\Trial.txt"),
            new StreamReader(@"C:\Users\anonymous\OneDrive\Desktop\SF2023\FEM_Code\data\QJEP paper data\Experiment 2\Composite.txt"),
            new StreamReader(@"C:\Users\anonymous\OneDrive\Desktop\SF2023\FEM_Code\data\QJEP paper data\Experiment 2\Strategy.txt"),
            new StreamReader(@"C:\Users\anonymous\OneDrive\Desktop\SF2023\FEM_Code\data\QJEP paper data\Experiment 2\Angle.txt"),
            new StreamReader(@"C:\Users\anonymous\OneDrive\Desktop\SF2023\FEM_Code\data\QJEP paper data\Experiment 2\Morph.txt"),
            new StreamReader(@"C:\Users\anonymous\OneDrive\Desktop\SF2023\FEM_Code\data\QJEP paper data\Experiment 2\Expected.txt"),
            new StreamReader(@"C:\Users\anonymous\OneDrive\Desktop\SF2023\FEM_Code\data\QJEP paper data\Experiment 2\CueType_NoDifference.txt"),      //this changes according to simulation #
            new StreamReader(@"C:\Users\anonymous\OneDrive\Desktop\SF2023\FEM_Code\data\QJEP paper data\Experiment 2\GazeProximity.txt") //this changes according to simulation #
        };
        
        // Headers for the output file
        outFile.WriteLine("Subject,Condition,Trial,CueType,Prediction,StrategySwitch,Strategy,JustShifted,StrategiesTried,PlanningPrep,Composite,PerformanceDip,BadShift,NoImprovement,TaskShift,AlreadyDoing,TaskTrial,Flexibility,NumComponents");

        // Read the data
        // int i = 0;
        while (true) {
            iterator++;
            try {
                string line0 = files[0].ReadLine();
                string line1 = files[1].ReadLine();
                string line2 = files[2].ReadLine();
                string line3 = files[3].ReadLine();
                string line4 = files[4].ReadLine();
                string line5 = files[5].ReadLine();
                string line6 = files[6].ReadLine();
                string line7 = files[7].ReadLine();
                string line8 = files[8].ReadLine(); //added for cue type
                string line9 = files[9].ReadLine(); //added for gaze proximity

                if (line0 == null || line1 == null || line2 == null || line3 == null || line4 == null || line5 == null || line6 == null || line7 == null || line8 == null || line9 == null) 
                { break;}
                

                Subject[iterator] = int.Parse(line0);
                Condition[iterator] = int.Parse(line1);
                Trial[iterator] = int.Parse(line2);
                Composite[iterator] = double.Parse(line3);
                if (Composite[iterator] > Composite[iterator - 1])
                    Score[iterator] = 1;
                else if (Composite[iterator] < Composite[iterator - 1])
                    Score[iterator] = -1;
                else
                    Score[iterator] = 0;
                Strategy[iterator] = line4;
                Angle[iterator] = int.Parse(line5);
                Morph[iterator] = int.Parse(line6);
                Expected[iterator] = line7;
                CueType[iterator] = double.Parse(line8);
                GazeProximity[iterator] = double.Parse(line9);
            } catch (IOException) {
                // Reached end of file
                break;
            }
        }

        // Close all the files
        foreach (StreamReader file in files) {
            if (file != null)
                file.Close();
        }


        RunSize = iterator - 1;
        iterator = 0;

        //Run the model
        while (iterator != RunSize)
        {
            iterator++;
            //update variables
            // For Isotope, the reset also applies to condition changes
            if (Subject[iterator] != PreviousSubject) //Reset at each new subject
            {
                string Label1;
                Prediction = 0;
                SubjectTrial = 0;
                PreviousTaskTrial = 0;
                NumStrategies = 0; //How many strategy shifts have there been?
                MinTime = 0;
                MaxTime = 0;
                WM = 4; //default working memory size
                Flexibility = 0;
                for (int j = 1; j <= 10; j++) //clear the buffer
                {
                    RecentTimes[j] = 0;
                }
                Label1 = "Subject:" + Subject[iterator].ToString(); //let me know what subject number the program is on
                //this.Update();
                //Label1.Show();
            }
            else
            {
                //do nothing
            }

            if (Condition[iterator] != PreviousCondition) //Reset at each new subject
            {
                PreviousTaskTrial = 0;
                MinTime = 0;
                MaxTime = 0;
                WM = 4; //default working memory size

                for (int j = 1; j <= 10; j++) //clear the bufferf
                {
                    RecentTimes[j] = 0;
                }
            }
            else
            {
                //do nothing
            }

            SubjectTrial = SubjectTrial + 1; //Keep track of which trial number it is for a given subject

            //predict strategy shift
            NumComponents = 0;
            TaskFactors();
            PersonBasedFactors();

            //Put prediction here.
            MakePrediction();

            //for the next trial for those components that use this information
            PreviousSubject = Subject[iterator];
            PreviousStrategy = Strategy[iterator];
            PreviousTaskTrial = TaskTrial;

        }

        //close the output files
        outFile.Close();

    }

    static void MakePrediction(){
        double[] alpha = new double[7];
        // Alpha(1) modifies serial position for a given subject on a task
        // Alpha(2) modifies the influence of the stimulus event. Default is 1
        // Alpha(3) modifies the influence of having just made a shift
        // Alpha(4) modifies the influence of the number of strategies tried
        // Alpha(5) modifies the influence a dip in performance
        // Alpha(6) modifies the degree to which a person is biased to be affected by a lack of improvement
        // alpha(7) modifies the influence of the cueing technique used.

        alpha[2] = 1; // default is 1

        // Make the prediction

        //Prediction = (alpha[2] * (TaskShift + AlreadyDoing)) + TaskTrial + JustShifted + StrategiesTried + PlanningPrep + PerformanceDip + CueInfluence + BadShift + NoImprovement + Flexibility;
        Prediction = JustShifted + PlanningPrep + CueInfluence + Flexibility;
        // Prediction = TaskShift;

        // Keep prediction no greater than 1 and no less than 0
        if (Prediction > 1) Prediction = 1;
        if (Prediction < 0) Prediction = 0;

        // Check to see if the person actually did switch strategies.
        if (Strategy[iterator] != PreviousStrategy && SubjectTrial != 1)
        {
            StrategySwitch = 1;
            NumStrategies++; // How many times has a person switched strategies
            LastSwitch = 1; // How long it has been since a strategy shift (used for JustShifted).
        }
        
        else
        {
            StrategySwitch = 0;
            LastSwitch++;
        }

        // There can be no strategy switch on the first trial
        if (SubjectTrial == 1)
        {
            Prediction = 0;
            StrategySwitch = 0;
        }

        // Figure Flexibility
        Flexibility = Flexibility + ((StrategySwitch - Prediction) / 10);

        outFile.WriteLine(Subject[iterator] + "," + Condition[iterator] + "," + Trial[iterator] + ", " + CueType[iterator] + ", " + Prediction + "," + StrategySwitch + "," + Strategy[iterator] + "," + JustShifted + "," + StrategiesTried + "," + PlanningPrep + "," + Composite[iterator] + "," + PerformanceDip + "," + BadShift + "," + NoImprovement + "," + TaskShift + "," + AlreadyDoing + "," + TaskTrial + "," + Flexibility + "," + NumComponents);

    }

    static void TaskFactors(){
        // These are changes in the task itself that would encourage a strategy shift.
        AssessTaskShift();
        AssessAlreadyDoing();
        AssessSerialPosition();
        AssessCueInfluence();
    }

    static void PersonBasedFactors(){
        AssessJustShifted();
        AssessStrategiesTried();
        AssessDip();
        AssessBadShift();
        AssessLackOfImprovement();
    }

    static void AssessTaskShift(){
        // The bigger the task change, the more likely a shift.
        // Figure 8 task specific
        // if (Condition[i] == 0) TaskShift = 0; // practice
        // if (Condition[i] == 1) TaskShift = 0; // baseline
        // if (Condition[i] == 2) TaskShift = 0; // draw random angle (varies depending on degree of angle)
        // if (Condition[i] == 2 && Trial[i] == 1) TaskShift = 0.1; // There is something different on the first trial
        // if (Condition[i] == 3) TaskShift = 0; // random angle trace (varies depending on degree of angle)

        TaskShift = Trial[iterator] == 1 ? 0.1 : 0;

        if (Morph[iterator] != 0)
        {
            TaskShift = Morph[iterator] * 0.05;
        }
        else
        {
            // do nothing
        }

        PreviousCondition = Condition[iterator];

        if (TaskShift != 0)
        {
            NumComponents++;
        }
    }

    static void AssessAlreadyDoing(){
        // This checks to see if a person is already doing the preferred strategy.
        AlreadyDoing = 0;
        // if (Condition[i] == 4 && PreviousStrategy == 1) AlreadyDoing = -TaskShift;
        // if (Condition[i] == 5 && PreviousStrategy == 3) AlreadyDoing = -TaskShift;
        // if (Condition[i] == 6 && PreviousStrategy == 2) AlreadyDoing = -TaskShift;
        // if (Condition[i] == 7 && PreviousStrategy == Expected[i]) AlreadyDoing = -TaskShift;

        if (PreviousStrategy == Expected[iterator])
        {
            AlreadyDoing = -TaskShift;
        }

        if (TaskShift != 0 && AlreadyDoing != 0)
        {
            NumComponents--;
        }
    }

    static void AssessSerialPosition(){
        // The longer it has been, the more likely a strategy shift.
        Alpha[0] = 0.001; // How fast this factor changes

        // This value increases, approaching 1, but never reaches 1
        TaskTrial = 1 - Math.Pow(1 + Alpha[0], -(SubjectTrial + 1));

        NumComponents++;

    }

    static void AssessJustShifted(){
        // If they just switched strategies, then they are less likely to switch again soon.
        // This value starts out high, but then gets less over time
        Alpha[2] = 0.01; // How fast this factor changes

        if (LastSwitch != 0 && TaskShift == 0) // Don't calcualte this if the task is forcing a shift anyway.
        {
            JustShifted = -(Alpha[2] * Math.Pow(LastSwitch, -(LastSwitch - 1)));
        }
        else
        {
            JustShifted = 0;
        }

        if (NumStrategies == 0) // There should not be a bias against shifting until it actually happens
        {
            JustShifted = 0;
        }

        if (Math.Abs(JustShifted) < 0.0001) // Getting to be too small to be meaningful
        {
            JustShifted = 0;
        }

        if (JustShifted != 0)
        {
            NumComponents++;
        }

    }

    static void AssessStrategiesTried(){
        // The more strategies a person has tried in the past, the less likely that are to try in the future.

        Alpha[3] = 0.001; // This is an arbitrary number. It may be that soem people are more senstive to this than others.

        StrategiesTried = -(1 - Math.Pow(1 + Alpha[3], -(NumStrategies - 1) - 1)); // This is a frustration factor.
        if (NumStrategies > 1)
        {
            NumComponents++;
        }

    }

    static void AssessDip(){
        //This part of the model will look for a performance dip as a signal to swtich strategies.
        //How well has the person been doing the past five trials (upper and lower range)?

        if (SubjectTrial > (WM + 1)) //5 trials is kind of arbitrary here. This is tied to working memory capacity.
        {
            for (int j = 2; j <= (WM + 1); j++)
            {
                RecentPerformance[j] = Composite[iterator - j];
            }

            MinPerformance = RecentPerformance[2];
            MaxPerformance = RecentPerformance[2];

            for (int j = 3; j <= (WM + 1); j++)
            {
                if (RecentPerformance[j] < MinPerformance) MinPerformance = RecentPerformance[j];
                if (RecentPerformance[j] > MaxPerformance) MaxPerformance = RecentPerformance[j];
            }
        }
        else
        {}

        //Is the current performance below the range?
        //The greater the dip, the greater the probability of a shift.

        Alpha[5] = 0.1;  //Again, arbitrary, and could vary between people

        if (SubjectTrial > (WM + 1))
        {
            if (Composite[iterator - 1] < MinPerformance)
            {
                HowBadIsIt = (MinPerformance - Composite[iterator - 1]) / (MaxPerformance - MinPerformance);
                PerformanceDip = 1 - Math.Pow(1 + Alpha[5], -HowBadIsIt - 1); //1.1 is arbitrary here.
            }
            else
            {
                PerformanceDip = 0;
            }
        }
        else //First 10 trials
        {
            PerformanceDip = 0;
        }

        if (PerformanceDip != 0) NumComponents = NumComponents + 1;
    }

    static void AssessBadShift(){
        //This is if there was a dip in performance as a result of a shift.
        //This suggests that it was a bad shift and that people should try to shift back.

        if (PerformanceDip != 0) //There was a performance dip
        {
            if (StrategySwitch == 1) //there was a strategy switch on the precioud trial
            {
                BadShift = PerformanceDip; //So double it (This is arbitrary here)
            }
            else
            {
                BadShift = 0;
            }
        }
        else
        {
            BadShift = 0;
        }

        if (BadShift != 0) NumComponents = NumComponents + 1;
    }

    static void AssessLackOfImprovement(){
        //if it has been a while without much improvement, try a new strategy

        slope = 0;  //reset

        Alpha[6] = 0.1;

        if (SubjectTrial > 5) //not meaningful until there have been a few trials
        {
            for (int j = iterator - 5; j <= iterator - 2; j++)
            {
                for (int k = j + 1; k <= iterator - 1; k++)
                {
                    slope += Composite[k] - Composite[j];
                }
            }

            slope /= 10;

            //If it is not positive, then swtich strategies.
			//This is a power function based on the lack of imporvement.

            if (slope < 0 && Strategy[iterator - 5] == Strategy[iterator - 4] && Strategy[iterator - 4] == Strategy[iterator - 3] &&
                Strategy[iterator - 3] == Strategy[iterator - 2] && Strategy[iterator - 2] == Strategy[iterator - 1])
            {
                NoImprovement = 1 - Math.Pow(1 + Alpha[6], slope);
            }
            else
            {
                NoImprovement = 0;
            }
        }

        else
        {
            NoImprovement = 0;
        }

        if (NoImprovement != 0)
        {
            NumComponents++;
        }
    }

    static void AssessCueInfluence(){
        //depending on which type of cue (overt or covert) is used, they are more likely to encounter an event boundary/switch actions
        
        Alpha[7] = 0.1; //this is arbitrary, some people may be more susceptible to cueing techniques than others
        double adjustedGazeProximity = GazeProximity[iterator];
        if (CueType[iterator] > 0.5)
        {
            adjustedGazeProximity = 1; //the dominant cue overrides gaze proximity
        }
        //CueType[iterator]
        //value depends on what type of cue it is, covert cues less likely to cause a shift (overt = 1, covert = 0.5)
        //gaze proximity is a multiplier (0.5-1.0), the close the gaze is the closer the number is to 1.0.
        CueInfluence = 1 - Math.Pow(1 + Alpha[7], -1*adjustedGazeProximity);
        
    }

    private void Button1_Click(object sender, EventArgs e){        
        Main();
    }


}
