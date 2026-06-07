#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CopyForSlotSprites : EditorWindow
{
    [MenuItem("Tools/Copy For DropSlot Sprites")]
    public static void ShowWindow()
    {
        GetWindow<CopyForSlotSprites>("Copy For DropSlot Sprites");
    }

    public DropSlot[] slots = new DropSlot[15];
    Vector2 scroll;

    void OnGUI()
    {
        GUILayout.Label("1. 把 15 個槽位依序拉入", EditorStyles.boldLabel);
        GUILayout.Label("2. 設定好每組第一個框的 Sprite");
        GUILayout.Label("   A=框1, B=框2, C=框3, D=框4, E=框13, F=框14");
        GUILayout.Label("3. 按下「複製 Sprite」");
        GUILayout.Space(10);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        for (int i = 0; i < 15; i++)
        {
            slots[i] = (DropSlot)EditorGUILayout.ObjectField(
                $"框 {i + 1}", slots[i], typeof(DropSlot), true);
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        if (GUILayout.Button("複製 Sprite", GUILayout.Height(40)))
            CopySprites();
    }

    void CopySprites()
    {
        // 各組：[來源, 目標...]
        int[][] groups = new int[][]
        {
            new int[] { 0 },                                          // A: 框1
            new int[] { 1 },                                          // B: 框2
            new int[] { 2 },                                          // C: 框3
            new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11 },               // D: 框4~12
            new int[] { 12 },                                         // E: 框13
            new int[] { 13, 14 },                                     // F: 框14~15
        };

        foreach (int[] group in groups)
        {
            DropSlot source = slots[group[0]];
            if (source == null) continue;

            for (int i = 1; i < group.Length; i++)
            {
                DropSlot target = slots[group[i]];
                if (target == null) continue;

                Undo.RecordObject(target, "Copy ForSlot Sprites");

                target.sprite_class        = source.sprite_class;
                target.sprite_main         = source.sprite_main;
                target.sprite_for          = source.sprite_for;
                target.sprite_int          = source.sprite_int;
                target.sprite_int_name     = source.sprite_int_name;
                target.sprite_equals       = source.sprite_equals;
                target.sprite_one          = source.sprite_one;
                target.sprite_int_count    = source.sprite_int_count;
                target.sprite_lessEqual    = source.sprite_lessEqual;
                target.sprite_ten          = source.sprite_ten;
                target.sprite_plusplus     = source.sprite_plusplus;
                target.sprite_print        = source.sprite_print;
                target.sprite_string_marks = source.sprite_string_marks;
                target.sprite_paddle_hard  = source.sprite_paddle_hard;
                target.defaultSprite       = source.defaultSprite;

                EditorUtility.SetDirty(target);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("複製完成！");
    }
}
#endif