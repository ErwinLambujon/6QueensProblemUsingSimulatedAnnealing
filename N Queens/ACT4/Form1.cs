﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections;

namespace ACT4
{
    public partial class Form1 : Form
    {
        double InitialTemperature = 300;
        double CoolingRate = 0.80;
        int side;
        int n = 6;
        SixState startState;
        SixState currentState;
        int moveCounter;
        Random random = new Random();

        //bool stepMove = true;

        int[,] hTable;
        ArrayList bMoves;
        Object chosenMove;


        public Form1()
        {
            InitializeComponent();

            side = pictureBox1.Width / n;

            startState = randomSixState();
            currentState = new SixState(startState);

            updateUI();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
        }

        private void updateUI()
        {
            //pictureBox1.Refresh();
            pictureBox2.Refresh();

            //label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
            label3.Text = "Attacking pairs: " + getAttackingPairs(currentState);
            label4.Text = "Moves: " + moveCounter;
            hTable = getHeuristicTableForPossibleMoves(currentState);
            bMoves = getBestMoves(hTable);

            listBox1.Items.Clear();
            foreach (Point move in bMoves)
            {
                listBox1.Items.Add(move);
            }

            if (bMoves.Count > 0)
                chosenMove = chooseMove(bMoves);
            label2.Text = "Chosen move: " + chosenMove;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // draw squares
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == startState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            // draw squares
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Black, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == currentState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private SixState randomSixState()
        {
            Random r = new Random();
            SixState random = new SixState(r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n));

            return random;
        }

        private int getAttackingPairs(SixState f)
        {
            int attackers = 0;

            for (int rf = 0; rf < n; rf++)
            {
                for (int tar = rf + 1; tar < n; tar++)
                {
                    // get horizontal attackers
                    if (f.Y[rf] == f.Y[tar])
                        attackers++;
                }
                for (int tar = rf + 1; tar < n; tar++)
                {
                    // get diagonal down attackers
                    if (f.Y[tar] == f.Y[rf] + tar - rf)
                        attackers++;
                }
                for (int tar = rf + 1; tar < n; tar++)
                {
                    // get diagonal up attackers
                    if (f.Y[rf] == f.Y[tar] + tar - rf)
                        attackers++;
                }
            }

            return attackers;
        }

        public void SolveSimulatedAnnealing()
        {
            Point randomAction = (Point)chooseMove(bMoves);
            SixState newlyCreatedState = new SixState(currentState);
            newlyCreatedState.Y[randomAction.X] = randomAction.Y;

            int valueOfE = getAttackingPairs(newlyCreatedState) - getAttackingPairs(currentState);
            double probability = Math.Exp(-valueOfE / InitialTemperature);

            if (valueOfE <= 0 || probability > random.NextDouble())
            {
                executeMove(randomAction);
            }

            InitialTemperature *= CoolingRate;
            Console.WriteLine(CoolingRate);
        }

        private int[,] getHeuristicTableForPossibleMoves(SixState thisState)
        {
            int[,] hStates = new int[n, n];

            for (int i = 0; i < n; i++) // go through the indices
            {
                for (int j = 0; j < n; j++) // replace them with a new value
                {
                    SixState possible = new SixState(thisState);
                    possible.Y[i] = j;
                    hStates[i, j] = getAttackingPairs(possible);
                }
            }

            return hStates;
        }

        private ArrayList getBestMoves(int[,] heuristicTable)
        {
            ArrayList bestMoves = new ArrayList();
            int bestHeuristicValue = heuristicTable[0, 0];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (bestHeuristicValue > heuristicTable[i, j])
                    {
                        bestHeuristicValue = heuristicTable[i, j];
                        bestMoves.Clear();
                        if (currentState.Y[i] != j)
                            bestMoves.Add(new Point(i, j));
                    }
                    else if (bestHeuristicValue == heuristicTable[i, j])
                    {
                        if (currentState.Y[i] != j)
                            bestMoves.Add(new Point(i, j));
                    }
                }
            }
            label5.Text = "Possible Moves (H=" + bestHeuristicValue + ")";
            return bestMoves;
        }

        private Object chooseMove(ArrayList possibleMoves)
        {
            int arrayLength = possibleMoves.Count;
            Random r = new Random();
            int randomMove = r.Next(arrayLength);

            return possibleMoves[randomMove];
        }

        private void executeMove(Point move)
        {
            for (int i = 0; i < n; i++)
            {
                startState.Y[i] = currentState.Y[i];
            }
            currentState.Y[move.X] = move.Y;
            moveCounter++;

            chosenMove = null;
            updateUI();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (getAttackingPairs(currentState) <= 0 || InitialTemperature <= 0)
                return;

            SolveSimulatedAnnealing();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            startState = randomSixState();
            currentState = new SixState(startState);

            moveCounter = 0;

            updateUI();
            pictureBox1.Refresh();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (true)
            {
                if (getAttackingPairs(currentState) <= 0 || InitialTemperature <= 0)
                    return;

                SolveSimulatedAnnealing();
            }
        }
    }
}