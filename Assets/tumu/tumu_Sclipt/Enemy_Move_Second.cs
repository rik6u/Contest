using UnityEngine;

public class Enemy_Move_Sevond : Enemy_System
{
    // Enemyが動くX座標の
    [Header("X座標の範囲")]
    [SerializeField] private float MaxX =  5.0f;
    [SerializeField] private float MinX = -5.0f;

    //  物理処理用の関数
    void FixedUpdate()
    {
        // 今の位置をPosに代入
        Vector3 Pos = Rb.position;

        // X座標を動かす
        Pos.x = MoveValue(Pos.x);

        // X座標の範囲チェック
        CheckRange(ref Pos.x, MaxX, MinX);

        // RigidbodyをPosの座標まで移動させる
        Rb.MovePosition(Pos);
    }
}