using UnityEngine;

public class SamplPlayer : MonoBehaviour
{
    // 移動速度
    [SerializeField] private float baseSpeed = 3f;

    // この秒数以上動けなかったら方向転換
    [SerializeField] private float stuckTimeLimit = 2f;

    // 上昇時の速度倍率（遅くなる）
    [SerializeField] private float upSpeedRate = 0.2f;

    // 下降時の速度倍率（速くなる）
    [SerializeField] private float downSpeedRate = 1.7f;

    //足場判定をするところ　＊Unity側で子オブジェクトとして使う
    [SerializeField] private Transform groundCheck;

    // 判定の大きさ
    [SerializeField] private float checkRadius = 0.15f;

    // 地面レイヤー
    [SerializeField] private LayerMask groundLayer;

    // 接地しているか
    public bool isGround;

    // Rigidbody2D
    private Rigidbody2D rb;

    // 移動方向
    // 1 = 右
    // -1 = 左
    private int moveDir = 1;

    // 前フレームのX,Y座標
    private float lastX;
    private float lastY;

    // 動けていない時間
    private float stuckTimer = 0f;

    // Animator
    private Animator anim;

    private void Start()
    {
        // Rigidbody取得
        rb = GetComponent<Rigidbody2D>();

        // 最初のX,Y座標保存
        lastX = transform.position.x;
        lastY = transform.position.y;

        //アニメーター
        anim = GetComponent<Animator>();

        
        //エラーログ
        if (rb == null )
        {
            Debug.Log("リジットボディーがありません");
        }
        if( groundCheck == null )
        {
            Debug.Log("足元判定が見当たりません");
        }
        if (anim == null)
        {
            Debug.Log("Animatorがありません");
        }

    }

    private void FixedUpdate()
    {
        // groundCheckの位置に円を作って、
        // 地面レイヤーに触れていたらtrue
        isGround = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // 今のX,Y座標
        float currentX = transform.position.x;
        float currentY = transform.position.y;

        // 今回使う速度（基本値からスタート）
        float currentSpeed = baseSpeed;




        //地面に触れているか
        if (isGround)
        {
            float yDifference = currentY - lastY;

            // 少しでも上がっている
            if (yDifference > 0.001f)
            {
                Debug.Log("登り");

                currentSpeed *= upSpeedRate;
            }
            // 少しでも下がっている
            else if (yDifference < -0.001f)
            {
                Debug.Log("下り");

                currentSpeed *= downSpeedRate;
            }
        }

        if (isGround)
        {
            // 地上でのみ移動
            rb.linearVelocity = new Vector2(currentSpeed * moveDir,rb.linearVelocity.y);

            // 地上でのみスタック判定
            if (Mathf.Abs(currentX - lastX) < 0.001f)
            {
                // 止まっている
                anim.SetBool("Move", false);
                anim.SetBool("Stop", true);

                // 詰まっている時間を加算
                stuckTimer += Time.fixedDeltaTime;
                Debug.Log(stuckTimer);

                // 指定時間以上詰まったら方向転換
                if (stuckTimer >= stuckTimeLimit)
                {
                    TurnAround();
                    stuckTimer = 0f;
                }
            }
            else
            {
                // 動いている
                anim.SetBool("Move", true);
                anim.SetBool("Stop", false);

                // 動けているならタイマーリセット
                stuckTimer = 0f;
            }
        }
        else
        {
            // 空中では方向転換しない
            stuckTimer = 0f;
        }

    //    //// 常に移動しようとする
    //    //rb.linearVelocity = new Vector2(currentSpeed * moveDir, rb.linearVelocity.y);

    //    //// X座標がほぼ変わっていなければ
    //    if (Mathf.Abs(currentX - lastX) < 0.001f)
    //    {
    //        // 止まっている
    //        anim.SetBool("Move", false);
    //        anim.SetBool("Stop", true);

    //        // 詰まっている時間を加算
    //        stuckTimer += Time.fixedDeltaTime;

    //        // 指定時間以上詰まったら方向転換
    //        if (stuckTimer >= stuckTimeLimit)
    //        {
    //            TurnAround();

    //            // タイマーリセット
    //            stuckTimer = 0f;
    //        }
    //    }
    //    else
    //    {
    //        // 動いている
    //        anim.SetBool("Move", true);
    //        anim.SetBool("Stop", false);

    //        // 動けているならタイマーリセット
    //        stuckTimer = 0f;
    //    }

        // 今回のX座標を保存
        lastX = currentX;
        lastY = currentY;    
        Debug.Log("今の速度: " + currentSpeed);
    }

    // 方向転換
    private void TurnAround()
    {
        // 1→-1、-1→1
        moveDir *= -1;

        // 見た目も反転
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -moveDir;    //元の画像が右に向いているなら　moveDirの-を消す
        transform.localScale = scale;
    }
}