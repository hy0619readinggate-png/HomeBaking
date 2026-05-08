using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng.Activity.C1_A04
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PaintTool : MonoBehaviour
    {
        // Properties
        public ToolType CurrentToolType => currentTool.ToolType;

        // Methods
        public void SelectDefault(ToolType toolType = ToolType.Pencil)
        {
            LOG.Info($"SelectDefault() | {toolType}", this);

            tools[toolType].IsSelected = true;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.interactable = enable;
            paletteCG.interactable = enable;
        }
        public void ShowPalette()
        {
            LOG.Info($"ShowPalette()", this);

            palettes[currentTool.ToolType].Show();
        }
        public void HidePalette()
        {
            LOG.Info($"HidePalette()", this);

            palettes[currentTool.ToolType].Hide();
        }
        public void TurnAffdance(bool on)
        {
            LOG.Info($"TurnAffdance() | {on}", this);

            var affs = GetComponentsInChildren<AffBase>();
            affs.ForEach(aff => aff.EnableAff = on);
        }


        // Events
        public event Action<ToolType, BrushInfo> OnToolChanged;



        // Fields : caching
        private CanvasGroup cg => GetComponent<CanvasGroup>();
        private CanvasGroup paletteCG => paletteSetGO.GetComponent<CanvasGroup>();

        // Fields
        private PaintToolItem currentTool = null;
        private Dictionary<ToolType, PaintToolItem> tools = null;
        private Dictionary<ToolType, Palette> palettes = null;

        // Event Handlers
        private void tool_OnSelected(PaintToolItem tool)
        {
            LOG.Info($"tool_OnSelected() | {tool.ToolType}", this);

            if (currentTool != null && currentTool != tool)
            {
                currentTool.IsSelected = false;
                palettes[currentTool.ToolType].Hide();
            }
            currentTool = tool;

            var currentPalette = palettes[currentTool.ToolType];
            currentPalette.Show();

            currentTool.Color = currentPalette.SelectedItem.BrushInfo.Color;

            OnToolChanged?.Invoke(currentTool.ToolType, currentPalette.SelectedItem.BrushInfo);
        }
        private void palette_OnItemSelected(PaletteItem item)
        {
            LOG.Info($"palette_OnItemSelected() | {item.BrushInfo}", this);

            currentTool.Color = item.BrushInfo.Color;
            // #30 선택시 바로 사라지지 않게 by veramocor
            //palettes[currentTool.ToolType].Hide();

            AudioMGR.One.PlayEffect(itemSelectCLIP);

            OnToolChanged?.Invoke(currentTool.ToolType, item.BrushInfo);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject paletteSetGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip itemSelectCLIP = null;


        // Unity Messages
        private void Awake()
        {
            tools = Util.GetDictionaryOfChildren<ToolType, PaintToolItem>(gameObject, t => t.ToolType);

            //tools.Values.ForEach(t => t.Color = )
            palettes = Util.GetDictionaryOfChildren<ToolType, Palette>(paletteSetGO, p => p.ToolType);
            palettes.Values.ForEach(p => p.gameObject.SetActive(true));

            foreach (var tool in tools.Values)
            {
                var toolType = tool.ToolType;
                tool.Color = palettes[toolType].SelectedItem.BrushInfo.Color;
            }
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            tools.Values.ForEach(t => t.OnSelected += tool_OnSelected);
            palettes.Values.ForEach(p => p.OnItemSelected += palette_OnItemSelected);
        }
        private void OnDisable()
        {
            tools.Values.ForEach(t => t.OnSelected -= tool_OnSelected);
            palettes.Values.ForEach(p => p.OnItemSelected -= palette_OnItemSelected);
        }


    }
}