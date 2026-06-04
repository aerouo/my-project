#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CopyDropSlotSprites : EditorWindow
{
    [MenuItem("Tools/Copy DropSlot Sprites")]
    public static void ShowWindow()
    {
        GetWindow<CopyDropSlotSprites>("Copy DropSlot Sprites");
    }

    // 每組的來源框（index 0-based）和目標框
    // A: 框1(0)
    // B: 框2(1)
    // C: 框3(2) → 8(7), 17(16), 24(23)
    // D: 框4(3) → 9(8), 13(12), 15(14), 16(15), 19(18), 20(19), 22(21), 23(22), 26(25), 27(26)
    // E: 框5(4) → 10(9)
    // F: 框6(5) → 7(6), 11(10), 18(17), 25(24)
    // G: 框12(11)
    // H: 框14(13) → 21(20)

    public DropSlot[] slots = new DropSlot[27];

    Vector2 scroll;

    void OnGUI()
    {
        GUILayout.Label("1. 把 27 個槽位依序拉入", EditorStyles.boldLabel);
        GUILayout.Label("2. 設定好每組第一個框的 Sprite");
        GUILayout.Label("3. 按下「複製 Sprite」");
        GUILayout.Space(10);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        for (int i = 0; i < 27; i++)
        {
            slots[i] = (DropSlot)EditorGUILayout.ObjectField(
                $"框 {i + 1}", slots[i], typeof(DropSlot), true);
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        if (GUILayout.Button("複製 Sprite", GUILayout.Height(40)))
        {
            CopySprites();
        }
    }

    void CopySprites()
    {
        // 定義每組：[來源index, 目標index...]
        int[][] groups = new int[][]
        {
            new int[] { 0 },                                              // A
            new int[] { 1 },                                              // B
            new int[] { 2, 7, 16, 23 },                                   // C
            new int[] { 3, 8, 12, 14, 15, 18, 19, 21, 22, 25, 26 },      // D
            new int[] { 4, 9 },                                           // E
            new int[] { 5, 6, 10, 17, 24 },                               // F
            new int[] { 11 },                                             // G
            new int[] { 13, 20 },                                         // H
        };

        foreach (int[] group in groups)
        {
            DropSlot source = slots[group[0]];
            if (source == null) continue;

            for (int i = 1; i < group.Length; i++)
            {
                DropSlot target = slots[group[i]];
                if (target == null) continue;

                Undo.RecordObject(target, "Copy DropSlot Sprites");

                target.sprite_class        = source.sprite_class;
                target.sprite_main         = source.sprite_main;
                target.sprite_print        = source.sprite_print;
                target.sprite_string       = source.sprite_string;
                target.sprite_string_marks = source.sprite_string_marks;
                target.sprite_string_name  = source.sprite_string_name;
                target.sprite_string_mark  = source.sprite_string_mark;
                target.sprite_equals       = source.sprite_equals;
                target.sprite_minus_x      = source.sprite_minus_x;
                target.sprite_minus        = source.sprite_minus;
                target.sprite_x            = source.sprite_x;
                target.sprite_char         = source.sprite_char;
                target.sprite_char_name    = source.sprite_char_name;
                target.sprite_blueprint    = source.sprite_blueprint;
                target.sprite_switch       = source.sprite_switch;
                target.sprite_case         = source.sprite_case;
                target.sprite_char_symbol  = source.sprite_char_symbol;
                target.sprite_place_wood   = source.sprite_place_wood;
                target.sprite_break        = source.sprite_break;
                target.sprite_binding_rope = source.sprite_binding_rope;
                target.defaultSprite       = source.defaultSprite;

                EditorUtility.SetDirty(target);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("複製完成！");
    }
}
#endif
