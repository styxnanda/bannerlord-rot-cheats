using System;
using System.Collections.Generic;
using RoTCheats.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace RoTCheats.Cheats
{
    public static class SkillCheats
    {
        public static readonly SkillObject[] AllSkills = new SkillObject[]
        {
            DefaultSkills.OneHanded,
            DefaultSkills.TwoHanded,
            DefaultSkills.Polearm,
            DefaultSkills.Bow,
            DefaultSkills.Crossbow,
            DefaultSkills.Throwing,
            DefaultSkills.Riding,
            DefaultSkills.Athletics,
            DefaultSkills.Crafting,
            DefaultSkills.Tactics,
            DefaultSkills.Scouting,
            DefaultSkills.Roguery,
            DefaultSkills.Charm,
            DefaultSkills.Leadership,
            DefaultSkills.Trade,
            DefaultSkills.Steward,
            DefaultSkills.Medicine,
            DefaultSkills.Engineering
        };

        public static void MaxAllSkills(Hero hero, int targetLevel = 300)
        {
            if (hero == null || hero.HeroDeveloper == null) return;

            foreach (SkillObject skill in AllSkills)
            {
                if (skill != null)
                {
                    hero.HeroDeveloper.SetInitialSkillLevel(skill, targetLevel);
                }
            }

            hero.HeroDeveloper.CheckLevel(false);

            CheatNotify.Show(string.Format("[RoT Cheats] Set all skills for {0} to {1}!", hero.Name.ToString(), targetLevel));
        }

        public static void AddSkillXpToAll(Hero hero, float xp = 50000f)
        {
            if (hero == null || hero.HeroDeveloper == null) return;

            foreach (SkillObject skill in AllSkills)
            {
                if (skill != null)
                {
                    hero.HeroDeveloper.AddSkillXp(skill, xp, false, true);
                }
            }

            CheatNotify.Show(string.Format("[RoT Cheats] Added +{0:N0} XP to all skills for {1}!", xp, hero.Name.ToString()));
        }

        public static void AddPoints(Hero hero, int focusPoints = 50, int attributePoints = 20)
        {
            if (hero == null || hero.HeroDeveloper == null) return;

            hero.HeroDeveloper.UnspentFocusPoints += focusPoints;
            hero.HeroDeveloper.UnspentAttributePoints += attributePoints;

            CheatNotify.Show(string.Format("[RoT Cheats] Added +{0} Focus Points and +{1} Attribute Points to {2}!", focusPoints, attributePoints, hero.Name.ToString()));
        }

        public static void LevelUp(Hero hero, int levels = 5)
        {
            if (hero == null || hero.HeroDeveloper == null) return;

            int newLevel = hero.Level + levels;
            hero.HeroDeveloper.SetInitialLevel(newLevel);
            hero.HeroDeveloper.CheckLevel(true);

            CheatNotify.Show(string.Format("[RoT Cheats] Leveled up {0} by +{1} levels (Now Level {2})!", hero.Name.ToString(), levels, hero.Level));
        }
    }
}
