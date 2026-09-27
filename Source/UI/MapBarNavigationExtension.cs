using System;
using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using Bannerlord.UIExtenderEx.ViewModels;
using RoTCheats.Cheats;
using RoTCheats.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace RoTCheats.UI
{
    [PrefabExtension("MapBar", "/Prefab/Window/Widget/Children")]
    public class MapBarNavigationEnhancerPrefabExtension : PrefabExtensionInsertPatch
    {
        public override InsertType Type
        {
            get { return InsertType.Child; }
        }

        private readonly List<XmlNode> _nodes = new List<XmlNode>();

        public MapBarNavigationEnhancerPrefabExtension()
        {
            XmlDocument doc = new XmlDocument();
            string xml = "<Children>" +
                "<Widget WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"160\" SuggestedHeight=\"36\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Top\" MarginRight=\"20\" MarginTop=\"135\">" +
                "  <Children>" +
                "    <ButtonWidget Id=\"NavigationEnhancerFloatingButton\" DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" SuggestedWidth=\"160\" SuggestedHeight=\"36\" Brush=\"ButtonBrush2\" Command.Click=\"ExecuteOpenNavigationEnhancer\" UpdateChildrenStates=\"true\">" +
                "      <Children>" +
                "        <TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" Brush=\"Kingdom.GeneralButtons.Text\" Brush.FontSize=\"14\" Brush.TextHorizontalAlignment=\"Center\" Brush.TextVerticalAlignment=\"Center\" Text=\"@NavigationEnhancerButtonLabel\" />" +
                "        <HintWidget DataSource=\"{NavigationEnhancerHint}\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" Command.HoverBegin=\"ExecuteBeginHint\" Command.HoverEnd=\"ExecuteEndHint\" />" +
                "      </Children>" +
                "    </ButtonWidget>" +
                "  </Children>" +
                "</Widget>" +
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
    public class MapBarNavigationEnhancerVMMixin : BaseViewModelMixin<MapBarVM>
    {
        public MapBarNavigationEnhancerVMMixin(MapBarVM vm) : base(vm)
        {
        }

        public override void OnRefresh()
        {
            base.OnRefresh();
            OnPropertyChanged("NavigationEnhancerButtonLabel");
            OnPropertyChanged("NavigationEnhancerHint");
        }

        [DataSourceProperty]
        public string NavigationEnhancerButtonLabel
        {
            get
            {
                float bonus = CheatSettings.Instance.PartyBaseSpeedBonus;
                if (Math.Abs(bonus) > 0.05f)
                {
                    return string.Format("Nav Enhancer ({0:+0.0;-0.0;0.0})", bonus);
                }
                return "Nav Enhancer";
            }
        }

        [DataSourceProperty]
        public HintViewModel NavigationEnhancerHint
        {
            get
            {
                float bonus = CheatSettings.Instance.PartyBaseSpeedBonus;
                CampaignVec2 dest;
                string destName;
                bool hasDest = NavigationEnhancerManager.HasDestination(out dest, out destName);

                string hint = string.Format(
                    "🧭 Navigation Enhancer\n• Party Base Speed Bonus: {0:+0.0;-0.0;0.0}\n• Destination: {1}\nClick to adjust party speed (+5/-5) or teleport to destination.",
                    bonus,
                    hasDest ? destName : "None (click map with Left Mouse)"
                );

                return new HintViewModel(new TextObject(hint));
            }
        }

        [DataSourceMethod]
        public void ExecuteOpenNavigationEnhancer()
        {
            NavigationEnhancerManager.OpenNavigationEnhancerMenu();
        }
    }
}
