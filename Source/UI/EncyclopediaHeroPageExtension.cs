using System;
using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using Bannerlord.UIExtenderEx.ViewModels;
using RoTCheats.Cheats;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace RoTCheats.UI
{
    [PrefabExtension("EncyclopediaHeroPage", "descendant::RichTextWidget[@Text='@InformationText']")]
    public class EncyclopediaHeroPageCheatsPrefabExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type
        {
            get { return InsertType.Append; }
        }

        private readonly List<XmlNode> _nodes = new List<XmlNode>();

        public EncyclopediaHeroPageCheatsPrefabExtension()
        {
            XmlDocument doc = new XmlDocument();
            string xml = "<Children>" +
                "<ListPanel HorizontalAlignment=\"Center\" HeightSizePolicy=\"CoverChildren\" WidthSizePolicy=\"CoverChildren\" MarginTop=\"10\" StackLayout.LayoutMethod=\"HorizontalLeftToRight\">" +
                "  <Children>" +
                "    <ButtonWidget DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"190\" SuggestedHeight=\"40\" MarginLeft=\"5\" MarginRight=\"5\" Brush=\"ButtonBrush2\" HorizontalAlignment=\"Left\" UpdateChildrenStates=\"true\" Command.Click=\"ExecuteLocateHero\" IsEnabled=\"@IsLocateHeroAvailable\">" +
                "      <Children>" +
                "        <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" Brush=\"Kingdom.GeneralButtons.Text\" Text=\"@LocateHeroActionName\" />" +
                "        <HintWidget DataSource=\"{LocateHeroHint}\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" Command.HoverBegin=\"ExecuteBeginHint\" Command.HoverEnd=\"ExecuteEndHint\" />" +
                "      </Children>" +
                "    </ButtonWidget>" +
                "    <ButtonWidget DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"150\" SuggestedHeight=\"40\" MarginLeft=\"5\" MarginRight=\"5\" Brush=\"ButtonBrush2\" HorizontalAlignment=\"Left\" UpdateChildrenStates=\"true\" Command.Click=\"ExecuteCheatExecuteHero\" IsEnabled=\"@IsExecuteHeroAvailable\">" +
                "      <Children>" +
                "        <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" Brush=\"Kingdom.GeneralButtons.Text\" Text=\"@ExecuteHeroActionName\" />" +
                "        <HintWidget DataSource=\"{ExecuteHeroHint}\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" Command.HoverBegin=\"ExecuteBeginHint\" Command.HoverEnd=\"ExecuteEndHint\" />" +
                "      </Children>" +
                "    </ButtonWidget>" +
                "  </Children>" +
                "</ListPanel>" +
                "</Children>";

            doc.LoadXml(xml);
            foreach (XmlNode child in doc.DocumentElement.ChildNodes)
            {
                _nodes.Add(child);
            }
        }

        [PrefabExtensionXmlNodes]
        public IEnumerable<XmlNode> GetNodes()
        {
            return _nodes;
        }
    }

    [ViewModelMixin("RefreshValues")]
    public class EncyclopediaHeroPageCheatsVMMixin : BaseViewModelMixin<EncyclopediaHeroPageVM>
    {
        private Hero _hero;

        public EncyclopediaHeroPageCheatsVMMixin(EncyclopediaHeroPageVM vm) : base(vm)
        {
            UpdateHero();
        }

        public override void OnRefresh()
        {
            base.OnRefresh();
            UpdateHero();
            OnPropertyChanged("LocateHeroActionName");
            OnPropertyChanged("IsLocateHeroAvailable");
            OnPropertyChanged("LocateHeroHint");
            OnPropertyChanged("ExecuteHeroActionName");
            OnPropertyChanged("IsExecuteHeroAvailable");
            OnPropertyChanged("ExecuteHeroHint");
        }

        private void UpdateHero()
        {
            if (ViewModel != null)
            {
                _hero = ViewModel.Obj as Hero;
            }
        }

        [DataSourceProperty]
        public string LocateHeroActionName
        {
            get { return "Locate in World"; }
        }

        [DataSourceProperty]
        public bool IsLocateHeroAvailable
        {
            get
            {
                UpdateHero();
                return _hero != null && !_hero.IsDead;
            }
        }

        [DataSourceProperty]
        public HintViewModel LocateHeroHint
        {
            get
            {
                UpdateHero();
                if (_hero != null && _hero.IsDead)
                {
                    return new HintViewModel(new TextObject("{=rot_locate_dead}Character is deceased. Cannot locate in world."));
                }
                return new HintViewModel(new TextObject("{=rot_locate_hint}Reveal and locate this character in the world view; camera will smoothly pan to their exact location."));
            }
        }

        [DataSourceProperty]
        public string ExecuteHeroActionName
        {
            get { return "Execute"; }
        }

        [DataSourceProperty]
        public bool IsExecuteHeroAvailable
        {
            get
            {
                UpdateHero();
                return _hero != null && !_hero.IsDead && _hero != Hero.MainHero;
            }
        }

        [DataSourceProperty]
        public HintViewModel ExecuteHeroHint
        {
            get
            {
                UpdateHero();
                if (_hero != null && _hero.IsDead)
                {
                    return new HintViewModel(new TextObject("{=rot_exec_dead}Character is already deceased."));
                }
                if (_hero == Hero.MainHero)
                {
                    return new HintViewModel(new TextObject("{=rot_exec_self}Cannot execute yourself."));
                }
                return new HintViewModel(new TextObject("{=rot_exec_hint}Execute this character: choose death by player character or natural causes."));
            }
        }

        [DataSourceMethod]
        public void ExecuteLocateHero()
        {
            UpdateHero();
            if (_hero == null) return;
            NavigationEnhancerManager.LocateHeroInWorld(_hero);
        }

        [DataSourceMethod]
        public void ExecuteCheatExecuteHero()
        {
            UpdateHero();
            if (_hero == null || _hero.IsDead) return;

            NavigationEnhancerManager.OpenExecuteHeroPrompt(_hero, delegate
            {
                if (ViewModel != null)
                {
                    ViewModel.RefreshValues();
                }
                OnPropertyChanged("IsLocateHeroAvailable");
                OnPropertyChanged("IsExecuteHeroAvailable");
                OnPropertyChanged("LocateHeroHint");
                OnPropertyChanged("ExecuteHeroHint");
            });
        }
    }
}
