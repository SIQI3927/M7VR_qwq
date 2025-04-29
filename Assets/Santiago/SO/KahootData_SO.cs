using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "KahootData", menuName = "SO/KahootData")]
public class KahootData_SO : ScriptableObject
{
    [SerializeField]
    public int number;
    [SerializeField]
    public List<Question> questions = new List<Question>()
    {
        new Question("¿Cuál es el metal más abundante en la corteza terrestre?", "Hierro,Aluminio,Cobre,Plata".Split(',').ToList(), 1),
        new Question("¿Qué matemático desarrolló el teorema que lleva su nombre sobre los triángulos rectángulos?", "Arquímedes,Pitágoras,Euclides,Newton".Split(',').ToList(), 1),
        new Question("¿Cuál es el gas más abundante en la atmósfera terrestre?", "Oxígeno,Nitrógeno,Dióxido de carbono,Hidrógeno".Split(',').ToList(), 1),
        new Question("¿Qué tipo de ondas utiliza el radar?", "Ultravioleta,Sonoras,Electromagnéticas,Infrarrojas".Split(',').ToList(), 2),
        new Question("¿Quién fue el primer emperador de Roma?", "Julio César,Augusto,Nerón,Trajano".Split(',').ToList(), 1),
        new Question("¿Cuál es el número atómico del oxígeno?", "6,7,8,9".Split(',').ToList(), 2),
        new Question("¿Quién formuló la ley de la gravitación universal?", "Kepler,Copérnico,Newton,Galileo".Split(',').ToList(), 2),
        new Question("¿Qué filamento se usaba en las primeras bombillas incandescentes?", "Cobre,Plomo,Tungsteno,Carbón".Split(',').ToList(), 3),
        new Question("¿Cuál es el elemento químico más ligero?", "Helio,Oxígeno,Hidrógeno,Nitrógeno".Split(',').ToList(), 2),
        new Question("¿En qué año terminó la Segunda Guerra Mundial?", "1940,1943,1945,1950".Split(',').ToList(), 2),
    };

    public string GetQuestionTitle(int currentQuestion)
    {
        return questions[currentQuestion].questionName;
    }

    public string GetAnswer(int currentQuestion, int currentAnswer)
    {
        return questions[currentQuestion].answerList[currentAnswer];
    }

    public int GetCorrectAnswer(int currentQuestion)
    {
        return questions[currentQuestion].correctAnswer;
    }
}
