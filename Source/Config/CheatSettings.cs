using System;

namespace RoTCheats.Config
{
    public class CheatSettings
    {
        private static CheatSettings _instance;
        public static CheatSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CheatSettings();
                }
                return _instance;
            }
        }

        private bool _godMode;
        private bool _partyGodMode;
        private bool _infiniteStamina = true;
        private bool _unlimitedAmmo;
        private bool _oneHitKill;
        private float _playerSpeedMultiplier = 1.0f;
        private float _squadSpeedMultiplier = 1.0f;
        private float _horseSpeedMultiplier = 1.0f;
        private bool _unlimitedWeight = true;
        private bool _enableBattleSkillHotkeys = true;
        private bool _enableLetterSkillKeys = true;
        private bool _enableNumpadSkillKeys = true;

        public bool EnableBattleSkillHotkeys
        {
            get { return MCMSettings.Instance != null ? MCMSettings.Instance.EnableBattleSkillHotkeys : _enableBattleSkillHotkeys; }
            set
            {
                if (MCMSettings.Instance != null) MCMSettings.Instance.EnableBattleSkillHotkeys = value;
                _enableBattleSkillHotkeys = value;
            }
        }

        public bool EnableLetterSkillKeys
        {
            get { return MCMSettings.Instance != null ? MCMSettings.Instance.EnableLetterSkillKeys : _enableLetterSkillKeys; }
            set
            {
                if (MCMSettings.Instance != null) MCMSettings.Instance.EnableLetterSkillKeys = value;
                _enableLetterSkillKeys = value;
            }
        }

        public bool EnableNumpadSkillKeys
        {
            get { return MCMSettings.Instance != null ? MCMSettings.Instance.EnableNumpadSkillKeys : _enableNumpadSkillKeys; }
            set
            {
                if (MCMSettings.Instance != null) MCMSettings.Instance.EnableNumpadSkillKeys = value;
                _enableNumpadSkillKeys = value;
            }
        }

        public bool GodMode
        {
            get { return MCMSettings.Instance != null ? MCMSettings.Instance.GodMode : _godMode; }
            set
            {
                if (MCMSettings.Instance != null) MCMSettings.Instance.GodMode = value;
                _godMode = value;
            }
        }

        public bool PartyGodMode
        {
            get { return MCMSettings.Instance != null ? MCMSettings.Instance.PartyGodMode : _partyGodMode; }
            set
            {
                if (MCMSettings.Instance != null) MCMSettings.Instance.PartyGodMode = value;
                _partyGodMode = value;
            }
        }

        public bool InfiniteStamina
        {
            get { return MCMSettings.Instance != null ? MCMSettings.Instance.InfiniteStamina : _infiniteStamina; }
            set
            {
                if (MCMSettings.Instance != null) MCMSettings.Instance.InfiniteStamina = value;
                _infiniteStamina = value;
            }
        }

        public bool UnlimitedAmmo
        {
            get { return MCMSettings.Instance != null ? MCMSettings.Instance.UnlimitedAmmo : _unlimitedAmmo; }
            set
            {
                if (MCMSettings.Instance != null) MCMSettings.Instance.UnlimitedAmmo = value;
                _unlimitedAmmo = value;
            }
        }

        public bool OneHitKill
        {
            get { return MCMSettings.Instance != null ? MCMSettings.Instance.OneHitKill : _oneHitKill; }
            set
            {
                if (MCMSettings.Instance != null) MCMSettings.Instance.OneHitKill = value;
                _oneHitKill = value;
            }
        }

        public float PlayerSpeedMultiplier
        {
            get { return MCMSettings.Instance != null ? MCMSettings.Instance.PlayerSpeedMultiplier : _playerSpeedMultiplier; }
            set
            {
                if (MCMSettings.Instance != null) MCMSettings.Instance.PlayerSpeedMultiplier = value;
                _playerSpeedMultiplier = value;
            }
        }

        public float SquadSpeedMultiplier
        {
            get { return MCMSettings.Instance != null ? MCMSettings.Instance.SquadSpeedMultiplier : _squadSpeedMultiplier; }
            set
            {
                if (MCMSettings.Instance != null) MCMSettings.Instance.SquadSpeedMultiplier = value;
                _squadSpeedMultiplier = value;
            }
        }

        public float HorseSpeedMultiplier
        {
            get { return MCMSettings.Instance != null ? MCMSettings.Instance.HorseSpeedMultiplier : _horseSpeedMultiplier; }
            set
            {
                if (MCMSettings.Instance != null) MCMSettings.Instance.HorseSpeedMultiplier = value;
                _horseSpeedMultiplier = value;
            }
        }

        public bool UnlimitedWeight
        {
            get { return MCMSettings.Instance != null ? MCMSettings.Instance.UnlimitedWeight : _unlimitedWeight; }
            set
            {
                if (MCMSettings.Instance != null) MCMSettings.Instance.UnlimitedWeight = value;
                _unlimitedWeight = value;
            }
        }

        public void Save()
        {
        }
    }
}
