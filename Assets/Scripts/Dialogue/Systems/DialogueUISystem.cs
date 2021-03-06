﻿using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Zoxel.UI;

namespace Zoxel
{

    // remove centrelinking from animated text as it looks weird (makes eyes move too much)
    // fade in individual text
    // randomize font location a little
    // lerp in font positions from the character talking
    // make sounds when they spawn too
    // change words to be different colours
    // Randomly generate textures for letters - use AI to generate them
    // load in meta for dialogue tree
    // once finished a line - can chose some lines depending on next informationn
    [DisableAutoCreation]

    public class DialogueUISpawnSystem : PlayerUISpawnSystem
    {
        public Dictionary<int, DialogueDatam> meta;

        #region Spawning-Removing
        public struct SpawnDialogueUI : IComponentData
        {
            public Entity character;
        }
        public struct RemoveDialogueUI : IComponentData
        {
            public Entity character;
        }
        public static void SpawnUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new SpawnDialogueUI { character = character });
        }
        public static void RemoveUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new RemoveDialogueUI { character = character });
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            Entities.WithAll<SpawnDialogueUI>().ForEach((Entity e, ref SpawnDialogueUI command) =>
            {
                SpawnUI(command.character, command); //, float2.zero);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveDialogueUI>().ForEach((Entity e, ref RemoveDialogueUI command) =>
            {
                RemoveUI(command.character);
                World.EntityManager.DestroyEntity(e);
            });
        }
        #endregion

        public override void OnClickedButton(Entity player, Entity ui, int arrayIndex, ButtonType buttonType)
        {
            DialogueUI dialogueUI = World.EntityManager.GetComponentData<DialogueUI>(ui);
            dialogueUI.confirmedChoice = (byte)(arrayIndex + 1); // 0 is unselected
            World.EntityManager.SetComponentData(ui, dialogueUI);
        }

        protected override void OnSpawnedPanel(Entity character, Entity panelUI, object spawnData)
        {
            var dialogueTree = Bootstrap.instance.data.dialogues[0].dialogueTree;
           //int startingDialogueID = dialogueTree.id;
            //DialogueDatam dialogueTree = meta[startingDialogueID];
            var branch = dialogueTree.branches[0];
            float fontSize = 0.03f;
            RenderText renderText = new RenderText
            {
                fontSize = fontSize,
                offsetX = ((-branch.speech.Length - 1f) / 2f) * fontSize
            };
            renderText.SetColor(Color.blue);
            DialogueUI dialogueUI = new DialogueUI
            {
                treeID = dialogueTree.id,
                timePerLetterMin = 0.07f,
                timePerLetterMax = 0.21f
            };
            dialogueUI.RandomizeCooldown();
            dialogueUI.SetText(branch.speech, ref renderText);
            //UnityEngine.Debug.LogError("Testing dialogue - self talking: " + dialogueTree.data[0].speech);
            World.EntityManager.AddComponentData(panelUI, dialogueUI);
            World.EntityManager.AddComponentData(panelUI, renderText); 
            byte uiIndex = ((byte)((int)PlayerUIType.DialogueUI));
            World.EntityManager.SetComponentData(panelUI, new PanelUI
            {
                id = uiIndex,
                characterID = World.EntityManager.GetComponentData<ZoxID>(character).id,
                size = new float2((branch.speech.Length) * (fontSize * 1.1f), fontSize),
                dirty = 1,
                orbitDepth = uiDatam.orbitDepth,
                anchor = (byte)UIAnchoredPosition.Middle
            });
            Childrens children = new Childrens { };
            World.EntityManager.AddComponentData(panelUI, children);

        }
    }
}
