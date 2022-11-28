﻿using System;
using System.Collections.Generic;
using NexusVisual.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NexusVisual.Editor
{
    internal class PlotSoEditorWindow : EditorWindow
    {
        private PlotSo _plotSo;
        private PlotSoGraphView _soGraphView;
        private string _title = "Plot Editor-";
        private static string _title2;
        private const KeyCode MenuKey = KeyCode.Space;
        private static PlotSoEditorWindow _window;

        [OnOpenAsset(1)]
        public static bool OnOpenAssets(int id, int line)
        {
            _window = GetWindow<PlotSoEditorWindow>();
            if (EditorUtility.InstanceIDToObject(id) is not PlotSo tree) return false;
            _title2 = $"{tree.name}";
            _window._plotSo = tree;
            _window.GraphViewInitialize();
            return true;
        }

        private void GraphViewInitialize()
        {
            var visualTree = EditorGUIUtility.Load("NodeEditorWindow.uxml") as VisualTreeAsset;
            if (!visualTree) throw new Exception("Can not find EditorWindow.uxml");
            visualTree.CloneTree(rootVisualElement);
            _soGraphView = rootVisualElement.Q<PlotSoGraphView>("GraphView");
            _soGraphView.RegisterCallback<KeyDownEvent>(MenuTrigger);
            _soGraphView.graphViewChanged += (_ =>
            {
                _title = "Plot Editor(Unsaved)-";
                return default;
            });

            var toolbarButton = rootVisualElement.Q<ToolbarButton>("Save");
            toolbarButton.clicked += DataSave;

            #region Re-draw the plot tree

            //Todo:Use cache
            var sectionDictionary = _plotSo.BaseSectionDic;
            var nodeDictionary = new Dictionary<string, Node>();

            foreach (var section in sectionDictionary.Values)
            {
                Node node;
                switch (section)
                {
                    case StartSection startSection:
                        node = new StartNode(startSection);
                        _soGraphView.AddElement(node);
                        nodeDictionary.Add(section.Guid, node);
                        break;
                    case DialogueSection dialogueSection:
                        node = new DialogueNode(dialogueSection);
                        _soGraphView.AddElement(node);
                        nodeDictionary.Add(section.Guid, node);
                        break;
                    default:
                        Debug.Log("Unknown BaseSection");
                        break;
                }
            }

            foreach (var section in sectionDictionary.Values)
            {
                if (!string.IsNullOrEmpty(section.Next))
                {
                    var edge = new Edge
                    {
                        output = nodeDictionary[section.Guid].outputContainer[0].Q<Port>(),
                        input = nodeDictionary[section.Next].inputContainer[0].Q<Port>()
                    };
                    edge.input.Connect(edge);
                    edge.output.Connect(edge);
                    _soGraphView.AddElement(edge);
                }
            }

            #endregion
        }

        private void MenuTrigger(KeyDownEvent keyDownEvent)
        {
            if (keyDownEvent.keyCode != MenuKey) return;
            //window left-top position + mouse relative position base on window left-top position
            var worldMousePosition = _window.position.position + keyDownEvent.originalMousePosition;
            var searchWindowContext = new SearchWindowContext(worldMousePosition);
            var searchWindowProvider = CreateInstance<NodeSearchWindowProvider>();
            searchWindowProvider.Info(_soGraphView);
            SearchWindow.Open(searchWindowContext, searchWindowProvider);
        }

        private void DataSave()
        {
            var collection = CreateInstance<PlotSo>();
            var edgeList = _soGraphView.edges.ToList();
            var nodeList = _soGraphView.nodes.ToList();

            #region Node

            foreach (var sectionNode in nodeList)
            {
                switch (sectionNode)
                {
                    case DialogueNode dialogueNode:
                        dialogueNode.data.Pos = dialogueNode.GetPosition();
                        collection.dialogueSections.Add(dialogueNode.data);
                        break;
                    case StartNode startNode:
                        startNode.data.Pos = startNode.GetPosition();
                        collection.startSections.Add(startNode.data);
                        break;
                    default:
                        Debug.Log("Unknown Node");
                        break;
                }
            }

            #endregion

            #region Link

            var sectionDictionary = collection.BaseSectionDic;
            foreach (var edge in edgeList)
            {
                var outputNode = edge.output.node;
                var inputNode = edge.input.node;
                if (
                    outputNode.userData is not BaseSection current ||
                    inputNode.userData is not BaseSection next
                ) continue;
                var thisGuid = current.Guid;
                var nextGuid = next.Guid;
                sectionDictionary[thisGuid].Next = nextGuid;
            }

            _plotSo.SectionCollect(sectionDictionary);
            EditorUtility.SetDirty(_plotSo);

            #endregion

            _title = "Plot Editor";
        }

        private void OnInspectorUpdate()
        {
            if (titleContent.text != _title + _title2)
                titleContent.text = _title + _title2;
        }
    }
}