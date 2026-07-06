using UnityEngine;

public class Enemy_Move_Fast : Enemy_System
{
    // Enemyが動くY座標の
    [Header("Y座標の範囲")]
    [SerializeField] private float MaxY = 5.0f;
    [SerializeField] private float MinY = -5.0f;

    //  物理処理用の関数
    void FixedUpdate()
    {
        // 今の位置をPosに代入
        Vector2 Pos = Rb.position;

        // Y座標を動かす
        Pos.y = MoveValue(Pos.y);

        // Y座標の範囲チェック
        CheckRange(ref Pos.y, MaxY, MinY);

        // RigidbodyをPosの座標まで移動させる
        Rb.MovePosition(Pos);
    }
}
