using System;

public static class GameStats
{
    public static int enemyKills;
    public static int shotsFired;
    public static int specialUsed;
    public static int powerUsed;
    public static int currentScore;

    public static void Reset()
    {
        enemyKills = 0;
        shotsFired = 0;
        specialUsed = 0;
        powerUsed = 0;
        currentScore = 0;

        HUD.GetInstance().scoreLabel.Text = currentScore.ToString();
    }

    public static void AddScore(int pAmount)
    {
        currentScore += pAmount;
    }

    public static void AddKill()
    {
        enemyKills++;
    }

    public static void AddShot()
    {
        shotsFired++;
    }

    public static void AddSpecial()
    {
        specialUsed++;
    }

    public static void AddPower()
    {
        powerUsed++;
    }
}
