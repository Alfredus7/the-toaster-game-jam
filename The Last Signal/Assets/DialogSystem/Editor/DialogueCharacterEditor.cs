using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DialogueCharacter))]
public class DialogueCharacterEditor : Editor
{
    private const float previewSize = 100f;
    private const float padding = 10f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DialogueCharacter character = (DialogueCharacter)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Portrait Preview", EditorStyles.boldLabel);

        if (character.portrait == null)
        {
            EditorGUILayout.HelpBox("No portrait assigned.", MessageType.Info);
        }
        else
        {
            DrawSpritePreview(character.portrait, previewSize, Color.gray);

            if (character.Material != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Portrait with Material", EditorStyles.boldLabel);
                DrawSpritePreview(character.portrait, previewSize, Color.black, character.Material);
            }
        }
    }

    private void DrawSpritePreview(Sprite sprite, float size, Color backgroundColor, Material mat = null)
    {
        // Área del rect para el sprite
        Rect rect = GUILayoutUtility.GetRect(size + padding, size + padding, GUILayout.ExpandWidth(false));

        // Dibuja fondo
        EditorGUI.DrawRect(rect, backgroundColor);

        // Calcula rect de sprite dentro del área
        Texture2D tex = sprite.texture;
        Rect texRect = sprite.textureRect;
        Rect uv = new Rect(
            texRect.x / tex.width,
            texRect.y / tex.height,
            texRect.width / tex.width,
            texRect.height / tex.height
        );

        Rect drawRect = new Rect(
            rect.x + padding / 2,
            rect.y + padding / 2,
            size,
            size
        );

        if (Event.current.type == EventType.Repaint)
        {
            if (mat != null)
            {
                mat.SetTexture("_MainTex", tex);
                Graphics.DrawTexture(drawRect, tex, uv, 0, 0, 0, 0, mat);
            }
            else
            {
                Graphics.DrawTexture(drawRect, tex, uv, 0, 0, 0, 0);
            }
        }
    }
}
