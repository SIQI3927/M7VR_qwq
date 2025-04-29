using System;
using System.Collections;
using System.Collections.Generic;

public class Question
{
    public string questionName;
    public List<string> answerList;
    public int correctAnswer;

    public Question(string questionName, List<string> answerList, int correctAnswer)
    {
        this.questionName = questionName;
        this.answerList = answerList;
        this.correctAnswer = correctAnswer;
    }
}
