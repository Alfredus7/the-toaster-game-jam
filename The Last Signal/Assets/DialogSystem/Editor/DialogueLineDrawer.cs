using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DialogueLine))]
public class DialogueLineDrawer : PropertyDrawer
{
    private const float portraitSize = 64f;
    private const float padding = 6f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Altura dinámica: retrato + texto
        return portraitSize + EditorGUIUtility.singleLineHeight * 4 + padding * 4;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty characterProp = property.FindPropertyRelative("character");
        SerializedProperty textProp = property.FindPropertyRelative("text");

        EditorGUI.BeginProperty(position, label, property);

        // Fondo ligeramente gris para separar líneas
        GUI.Box(position, GUIContent.none);

        // Margen interior
        position.x += padding;
        position.width -= padding * 2;
        position.y += padding;

        // --- FILA: Portrait + Character Name ---
        Rect portraitRect = new Rect(position.x, position.y, portraitSize, portraitSize);
        Rect nameRect = new Rect(position.x + portraitSize + padding, position.y, position.width - portraitSize - padding, EditorGUIUtility.singleLineHeight);

        // Dibujar sprite recortado
        Sprite sprite = GetPortrait(characterProp);
        if (sprite != null)
            DrawSprite(sprite, portraitRect);
        else
            EditorGUI.HelpBox(portraitRect, "No portrait", MessageType.None);

        // Nombre del personaje
        DialogueCharacter character = characterProp.objectReferenceValue as DialogueCharacter;
        string charName = (character != null) ? character.characterName : "Sin personaje";
        EditorGUI.LabelField(nameRect, charName, EditorStyles.boldLabel);

        // --- Campo: Character ---
        Rect charFieldRect = new Rect(nameRect.x, nameRect.y + EditorGUIUtility.singleLineHeight + padding, nameRect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(charFieldRect, characterProp, GUIContent.none);

        // --- Campo: Texto del diálogo ---
        Rect textRect = new Rect(position.x, position.y + portraitSize + padding * 2, position.width, EditorGUIUtility.singleLineHeight * 4);
        EditorGUI.PropertyField(textRect, textProp);

        EditorGUI.EndProperty();
    }

    private Sprite GetPortrait(SerializedProperty characterProp)
    {
        if (characterProp.objectReferenceValue == null) return null;
        DialogueCharacter dc = characterProp.objectReferenceValue as DialogueCharacter;
        return dc != null ? dc.portrait : null;
    }

    // Dibuja correctamente solo el rect del sprite (soporta spritesheets)
    private void DrawSprite(Sprite sprite, Rect rect)
    {
        if (sprite == null) return;

        Texture2D tex = sprite.texture;
        Rect spriteRect = sprite.textureRect;

        Rect uv = new Rect(
            spriteRect.x / tex.width,
            spriteRect.y / tex.height,
            spriteRect.width / tex.width,
            spriteRect.height / tex.height
        );

        GUI.DrawTextureWithTexCoords(rect, tex, uv);
    }
}
