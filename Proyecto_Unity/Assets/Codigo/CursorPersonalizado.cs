using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class CursorPersonalizado : MonoBehaviour
{
    public static CursorPersonalizado Instancia { get; private set; }

    [SerializeField] private RectTransform iconoCursor;
    
    void Awake()
    {
        if (Instancia != null)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);

        Cursor.visible = false;

        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // por encima de cualquier otro Canvas
    }

    void Update()
    {
        iconoCursor.position = Input.mousePosition;
    }
}