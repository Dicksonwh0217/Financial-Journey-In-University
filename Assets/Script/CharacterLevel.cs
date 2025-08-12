using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Charisma,
    Intelligence,
    MarketAnalysis,
    Entrepreneurship,
    Famous,
    Study
}

[Serializable]

public class Skill
{
    public int level = 1;
    public int experience = 0;

    public int NextLevel
    {
        get
        {
            return level * 1000;
        }
    }

    public SkillType skillType;

    public Skill(SkillType skillType)
    {
        level = 1;
        experience = 0;
        this.skillType = skillType;
    }

    public void AddExperience(int experience)
    {
        this.experience += experience;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        if (experience >= NextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        experience -= NextLevel;
        level += 1;
    }
}

public class CharacterLevel : MonoBehaviour
{
    [SerializeField] Skill Charisma;
    [SerializeField] Skill Intelligence;
    [SerializeField] Skill MarketAnalysis;
    [SerializeField] Skill Entrepreneurship;
    [SerializeField] Skill Famous;
    [SerializeField] Skill Study;

    private void Start()
    {
        Charisma = new Skill(SkillType.Charisma);
        Intelligence = new Skill(SkillType.Charisma);
        MarketAnalysis = new Skill(SkillType.Charisma);
        Entrepreneurship = new Skill(SkillType.Charisma);
        Famous = new Skill(SkillType.Charisma);
        Study = new Skill(SkillType.Charisma);
    }

    public int GetLevel(SkillType skillType)
    {
        Skill s = GetSkill(skillType);

        if (s == null)
        {
            return -1;
        }

        return s.level;
    }

    public void AddExperience(SkillType skillType, int experience)
    {
        Skill s = GetSkill(skillType);
        s.AddExperience(experience);
    }

    public Skill GetSkill(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Charisma:
                return Charisma;
            case SkillType.Intelligence:
                return Intelligence;
            case SkillType.MarketAnalysis:
                return MarketAnalysis;
            case SkillType.Entrepreneurship:
                return Entrepreneurship;
            case SkillType.Famous:
                return Famous;
            case SkillType.Study:
                return Study;
        }
        return null;
    }
}
