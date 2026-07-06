using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
// abstractを付けると、Enemy_System自体は直接オブジェクトに付けずに共通処理専用の親クラスとして使える
public abstract class Enemy_System : MonoBehaviour
{
    //  インスペクターでいじれつやつー
    // オブジェクトの移動速度
    [Header("移動速度")]
    [Range(0.0f, 10.0f)]
    [SerializeField] protected float Speed = 3.0f;

    [SerializeField] protected LayerMask HitLayer;

    // Enemyの動く向きの変数   1:上、右 -1:下、左
    protected int Direction = -1;

    // Rigidbodyを入れる変数
    protected Rigidbody2D Rb;

    protected bool isStop = false;

    /** @brief(何かしらのざっとした内容)	Rigidbodyを取得する関数
    *	@param(引数を描く)	float Value
    *	@return(戻り値)		Value + Speed * Direction * Time.fixedDeltaTime
    *	@date(作った日)		2026/05/12
    *	@memo(メモ)			
    */
    protected virtual void Awake()
    {
        //  このスクリプトがついているオブジェクトからRigidbodyを取得し変数に代入する
        Rb = GetComponent<Rigidbody2D>();
    }

    /** @brief(何かしらのざっとした内容)	移動量計算関数
    *	@param(引数を描く)	float Value
    *	@return(戻り値)		Value + Speed * Direction * Time.fixedDeltaTime
    *	@date(作った日)		2026/05/12
    *	@memo(メモ)			
    */
    protected float MoveValue(float Value)
    {
        //  止まっている時動かない
        if (isStop == true)
        {
            return Value;
        }

        // Enemyの移動する量
        // Time.fixedDeltaTime：これはUpdate関数が1回呼ばれるまでの時間
        return Value + Speed * Direction * Time.fixedDeltaTime;
    }

    /** @brief(何かしらのざっとした内容)	向き反転関数
    *	@param(引数を描く)	
    *	@return(戻り値)		void
    *	@date(作った日)		2026/05/12
    *	@memo(メモ)			Direction = Direction * -1
    */
    protected void TurnDirection()
    {
        Direction *= -1;
    }

    /** @brief(何かしらのざっとした内容)	範囲で向きを変える関数
	*	@param(引数を描く)	ref float Value, float Max, float Min   nowの座標と動ける範囲
	*	@return(戻り値)		void
	*	@date(作った日)		2026/05/12
	*	@memo(メモ)			-1が-座標 1が+座標
	*/
    protected void CheckRange(ref float Value, float Max, float Min)
    {
        if (isStop == true)
        {
            return;
        }

        if (Value >= Max)
        {
            Value = Max;
            // 進む向きを変更
            Direction = -1;
        }
        else if (Value <= Min)
        {
            Value = Min;
            // 進む向きを変更
            Direction = 1;
        }
    }

    /** @brief(何かしらのざっとした内容)	Steam_Line Tagに振れると反転する関数
    *	@param(引数を描く)	ref float Value, float Max, float Min   nowの座標と動ける範囲
    *	@return(戻り値)		void
    *	@date(作った日)		2026/05/12
    *	@memo(メモ)			線にぶつかったとき(特定のTagのオブジェクトに接触したとき)Triggerの当たり判定に入った瞬間関数が呼ばれる
    *	Collider other はぶつかった相手のCollider情報を取れるので相手のTagも取得できる
    */
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("何かに当たった：" + collision.gameObject.name);

        if (collision.gameObject.layer == LayerMask.NameToLayer("Steam_Line"))
        {
            Direction *= -1;

            Debug.Log("Wallに当たったので反転");
        }
    }

    protected bool IsHitLayer(GameObject HitObject, string LayerName)
    {
        return HitObject.layer == LayerMask.NameToLayer("Steam_Line");
    }

    protected void DestroyWhenInvisible()
    {
        Destroy(gameObject);
    }
}