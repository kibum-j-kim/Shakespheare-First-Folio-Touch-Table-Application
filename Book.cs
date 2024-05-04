//The implementation is based on this article:http://rbarraza.com/html5-canvas-pageflip/
//As the rbarraza.com website is not live anymore you can get an archived version from web archive 
//or check an archived version that I uploaded on my website: https://dandarawy.com/html5-canvas-pageflip/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
public enum FlipMode
{
    RightToLeft,
    LeftToRight
}
[ExecuteInEditMode]
public class Book : MonoBehaviour
{
    public Canvas canvas;
    public GameObject content;
    [SerializeField]
    RectTransform BookPanel;
    public Sprite background;
    public GameObject idleStatePanel1;
    public GameObject idleStatePanel2;
    public GameObject VideoGalleryPanel;
    public Sprite[] bookPages;
    public bool interactable = true;
    public bool enableShadowEffect = true;
    public float countdownTime = 300f; // Time in seconds for countdown to return back to idle state
    private float currentTime; // Current time 
    //represent the index of the sprite shown in the right page
    public int currentPage = 0;
    public int TotalPageCount
    {
        get { return bookPages.Length; }
    }
    public Vector3 EndBottomLeft
    {
        get { return ebl; }
    }
    public Vector3 EndBottomRight
    {
        get { return ebr; }
    }
    public float Height
    {
        get
        {
            return BookPanel.rect.height;
        }
    }
    public Image ClippingPlane;
    public Image NextPageClip;
    public Image Shadow;
    public Image ShadowLTR;
    public Image Left;
    public Image LeftNext;
    public Image Right;
    public Image RightNext;
    public GameObject RightHotSpot;
    public GameObject LeftHotSpot;
    // next and previous page buttons
    public Button btn_next;
    public Button btn_prev;
    public Button btn_quit;
    public Scrollbar pageScrollBar;
    public Text playNameText; // Current page name
    public Text page_num;
    public UnityEvent OnFlip;
    // timers for delay
    float touchTimer;
    float timerInterval = 0.1f;
    float radius1, radius2;
    //Spine Bottom
    Vector3 sb;
    //Spine Top
    Vector3 st;
    //corner of the page
    Vector3 c;
    //Edge Bottom Right
    Vector3 ebr;
    //Edge Bottom Left
    Vector3 ebl;
    //follow point 
    Vector3 f;
    //bool to check if page is currently dragging
    bool pageDragging = false;
    //bool to check if flip 10 or 50 is active
    bool flip10 = false;
    bool flip50 = false;
    //current flip mode
    FlipMode mode;

    [System.Serializable]
    public class PlayData
    {
        public string Category;
        public string Title;
        public int PageStart;
        public int PageEnd;
    }

    private List<PlayData> plays = new List<PlayData>();


    void Start()
    {
        PlayDataRepo();
        currentPage = 0; // Starting on the first page
        pageScrollBar.value = 0;
        UpdatePlayNameText();
        currentTime = countdownTime; // Initiatlize timer
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!canvas) Debug.LogError("Book should be a child to canvas");

        // enable the buttons
        btn_next.enabled = true;
        btn_prev.enabled = true;

        Left.gameObject.SetActive(false);
        Right.gameObject.SetActive(false);
        UpdateSprites();
        CalcCurlCriticalPoints();

        float pageWidth = BookPanel.rect.width / 2.0f;
        float pageHeight = BookPanel.rect.height;
        NextPageClip.rectTransform.sizeDelta = new Vector2(pageWidth, pageHeight + pageHeight * 2);


        ClippingPlane.rectTransform.sizeDelta = new Vector2(pageWidth * 2 + pageHeight, pageHeight + pageHeight * 2);

        //hypotenous (diagonal) page length
        float hyp = Mathf.Sqrt(pageWidth * pageWidth + pageHeight * pageHeight);
        float shadowPageHeight = pageWidth / 2 + hyp;

        Shadow.rectTransform.sizeDelta = new Vector2(pageWidth, shadowPageHeight);
        Shadow.rectTransform.pivot = new Vector2(1, (pageWidth / 2) / shadowPageHeight);

        ShadowLTR.rectTransform.sizeDelta = new Vector2(pageWidth, shadowPageHeight);
        ShadowLTR.rectTransform.pivot = new Vector2(0, (pageWidth / 2) / shadowPageHeight);

        pageScrollBar.onValueChanged.AddListener(OnScrollbarValueChange);
    }

    void PlayDataRepo()
    {
        var plays = new List<PlayData>
    {
        // Comedies
        new PlayData { Category = "Comedies", Title = "The Tempest", PageStart = 1, PageEnd = 19 },
        new PlayData { Category = "Comedies", Title = "The Two Gentlemen of Verona", PageStart = 20, PageEnd = 38 },
        new PlayData { Category = "Comedies", Title = "The Merry Wives of Windsor", PageStart = 39, PageEnd = 57 },
        new PlayData { Category = "Comedies", Title = "Measure for Measure", PageStart = 58, PageEnd = 76 },
        new PlayData { Category = "Comedies", Title = "The Comedy of Errors", PageStart = 77, PageEnd = 95 },
        new PlayData { Category = "Comedies", Title = "Much Ado About Nothing", PageStart = 96, PageEnd = 114 },
        new PlayData { Category = "Comedies", Title = "Love's Labour's Lost", PageStart = 115, PageEnd = 133 },
        new PlayData { Category = "Comedies", Title = "A Midsummer Night's Dream", PageStart = 134, PageEnd = 152 },
        new PlayData { Category = "Comedies", Title = "The Merchant of Venice", PageStart = 153, PageEnd = 171 },
        new PlayData { Category = "Comedies", Title = "As You Like It", PageStart = 172, PageEnd = 190 },
        new PlayData { Category = "Comedies", Title = "The Taming of the Shrew", PageStart = 191, PageEnd = 209 },
        new PlayData { Category = "Comedies", Title = "All's Well That Ends Well", PageStart = 210, PageEnd = 228 },
        new PlayData { Category = "Comedies", Title = "Twelfth Night", PageStart = 229, PageEnd = 247 },
        new PlayData { Category = "Comedies", Title = "The Winter's Tale", PageStart = 248, PageEnd = 266 },

        // Histories
        new PlayData { Category = "Histories", Title = "King John", PageStart = 267, PageEnd = 285 },
        new PlayData { Category = "Histories", Title = "Richard II", PageStart = 286, PageEnd = 304 },
        new PlayData { Category = "Histories", Title = "Henry IV, Part 1", PageStart = 305, PageEnd = 323 },
        new PlayData { Category = "Histories", Title = "Henry IV, Part 2", PageStart = 324, PageEnd = 342 },
        new PlayData { Category = "Histories", Title = "Henry V", PageStart = 343, PageEnd = 361 },
        new PlayData { Category = "Histories", Title = "Henry VI, Part 1", PageStart = 362, PageEnd = 380 },
        new PlayData { Category = "Histories", Title = "Henry VI, Part 2", PageStart = 381, PageEnd = 399 },
        new PlayData { Category = "Histories", Title = "Henry VI, Part 3", PageStart = 400, PageEnd = 418 },
        new PlayData { Category = "Histories", Title = "Richard III", PageStart = 419, PageEnd = 437 },
        new PlayData { Category = "Histories", Title = "Henry VIII", PageStart = 438, PageEnd = 456 },

        // Tragedies
        new PlayData { Category = "Tragedies", Title = "Troilus and Cressida", PageStart = 457, PageEnd = 475 },
        new PlayData { Category = "Tragedies", Title = "Coriolanus", PageStart = 476, PageEnd = 494 },
        new PlayData { Category = "Tragedies", Title = "Titus Andronicus", PageStart = 495, PageEnd = 513 },
        new PlayData { Category = "Tragedies", Title = "Romeo and Juliet", PageStart = 514, PageEnd = 532 },
        new PlayData { Category = "Tragedies", Title = "Timon of Athens", PageStart = 533, PageEnd = 551 },
        new PlayData { Category = "Tragedies", Title = "Julius Caesar", PageStart = 552, PageEnd = 570 },
        new PlayData { Category = "Tragedies", Title = "Macbeth", PageStart = 571, PageEnd = 589 },
        new PlayData { Category = "Tragedies", Title = "Hamlet", PageStart = 590, PageEnd = 608 },
        new PlayData { Category = "Tragedies", Title = "King Lear", PageStart = 609, PageEnd = 627 },
        new PlayData { Category = "Tragedies", Title = "Othello", PageStart = 628, PageEnd = 646 },
        new PlayData { Category = "Tragedies", Title = "Antony and Cleopatra", PageStart = 647, PageEnd = 665 },
        new PlayData { Category = "Tragedies", Title = "Cymbeline", PageStart = 666, PageEnd = 684 },
    };
    }
    public void ToggleGameObject(GameObject panel)
    {
        if (panel != null)
        {
            bool isActive = panel.activeSelf;
            panel.SetActive(!isActive);
        }
    }

    private void CalcCurlCriticalPoints()
    {
        sb = new Vector3(0, -BookPanel.rect.height / 2);
        ebr = new Vector3(BookPanel.rect.width / 2, -BookPanel.rect.height / 2);
        ebl = new Vector3(-BookPanel.rect.width / 2, -BookPanel.rect.height / 2);
        st = new Vector3(0, BookPanel.rect.height / 2);
        radius1 = Vector2.Distance(sb, ebr);
        float pageWidth = BookPanel.rect.width / 2.0f;
        float pageHeight = BookPanel.rect.height;
        radius2 = Mathf.Sqrt(pageWidth * pageWidth + pageHeight * pageHeight);
    }

    public Vector3 transformPoint(Vector3 mouseScreenPos)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector3 mouseWorldPos = canvas.worldCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, canvas.planeDistance));
            Vector2 localPos = BookPanel.InverseTransformPoint(mouseWorldPos);

            return localPos;
        }
        else if (canvas.renderMode == RenderMode.WorldSpace)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 globalEBR = transform.TransformPoint(ebr);
            Vector3 globalEBL = transform.TransformPoint(ebl);
            Vector3 globalSt = transform.TransformPoint(st);
            Plane p = new Plane(globalEBR, globalEBL, globalSt);
            float distance;
            p.Raycast(ray, out distance);
            Vector2 localPos = BookPanel.InverseTransformPoint(ray.GetPoint(distance));
            return localPos;
        }
        else
        {
            //Screen Space Overlay
            Vector2 localPos = BookPanel.InverseTransformPoint(mouseScreenPos);
            return localPos;
        }
    }
    void Update()
    {
        if (pageDragging && interactable)
        {
            btn_next.enabled = false;
            btn_prev.enabled = false;
            UpdateBook();
            currentTime = countdownTime;
        }
        else if (interactable) // delete
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                idleStatePanel1.SetActive(true);
                idleStatePanel2.SetActive(true);
                VideoGalleryPanel.SetActive(false);
                GoToPage(0);
            }
        }
    }

    // show current Play name
    void UpdatePlayNameText()
    {
        foreach (PlayData play in plays)
        {
            if (currentPage >= play.PageStart && currentPage <= play.PageEnd)
            {
                playNameText.text = play.Title;
                return;
            }
        }
    }

    public void ResetCountdownTimer()
    {
        currentTime = countdownTime;
    }
    public void UpdateBook()
    {
        f = Vector3.Lerp(f, transformPoint(Input.mousePosition), Time.deltaTime * 10);
        if (mode == FlipMode.RightToLeft)
            UpdateBookRTLToPoint(f);
        else
            UpdateBookLTRToPoint(f);
    }
    public void UpdateBookLTRToPoint(Vector3 followLocation)
    {
        mode = FlipMode.LeftToRight;
        f = followLocation;
        ShadowLTR.transform.SetParent(ClippingPlane.transform, true);
        ShadowLTR.transform.localPosition = new Vector3(0, 0, 0);
        ShadowLTR.transform.localEulerAngles = new Vector3(0, 0, 0);
        Left.transform.SetParent(ClippingPlane.transform, true);

        Right.transform.SetParent(BookPanel.transform, true);
        Right.transform.localEulerAngles = Vector3.zero;
        LeftNext.transform.SetParent(BookPanel.transform, true);

        c = Calc_C_Position(followLocation);
        Vector3 t1;
        float clipAngle = CalcClipAngle(c, ebl, out t1);
        //0 < T0_T1_Angle < 180
        clipAngle = (clipAngle + 180) % 180;

        ClippingPlane.transform.localEulerAngles = new Vector3(0, 0, clipAngle - 90);
        ClippingPlane.transform.position = BookPanel.TransformPoint(t1);

        //page position and angle
        Left.transform.position = BookPanel.TransformPoint(c);
        float C_T1_dy = t1.y - c.y;
        float C_T1_dx = t1.x - c.x;
        float C_T1_Angle = Mathf.Atan2(C_T1_dy, C_T1_dx) * Mathf.Rad2Deg;
        Left.transform.localEulerAngles = new Vector3(0, 0, C_T1_Angle - 90 - clipAngle);

        NextPageClip.transform.localEulerAngles = new Vector3(0, 0, clipAngle - 90);
        NextPageClip.transform.position = BookPanel.TransformPoint(t1);
        LeftNext.transform.SetParent(NextPageClip.transform, true);
        Right.transform.SetParent(ClippingPlane.transform, true);
        Right.transform.SetAsFirstSibling();

        ShadowLTR.rectTransform.SetParent(Left.rectTransform, true);
    }
    public void UpdateBookRTLToPoint(Vector3 followLocation)
    {
        mode = FlipMode.RightToLeft;
        f = followLocation;
        Shadow.transform.SetParent(ClippingPlane.transform, true);
        Shadow.transform.localPosition = Vector3.zero;
        Shadow.transform.localEulerAngles = Vector3.zero;
        Right.transform.SetParent(ClippingPlane.transform, true);

        Left.transform.SetParent(BookPanel.transform, true);
        Left.transform.localEulerAngles = Vector3.zero;
        RightNext.transform.SetParent(BookPanel.transform, true);
        c = Calc_C_Position(followLocation);
        Vector3 t1;
        float clipAngle = CalcClipAngle(c, ebr, out t1);
        if (clipAngle > -90) clipAngle += 180;

        ClippingPlane.rectTransform.pivot = new Vector2(1, 0.35f);
        ClippingPlane.transform.localEulerAngles = new Vector3(0, 0, clipAngle + 90);
        ClippingPlane.transform.position = BookPanel.TransformPoint(t1);

        //page position and angle
        Right.transform.position = BookPanel.TransformPoint(c);
        float C_T1_dy = t1.y - c.y;
        float C_T1_dx = t1.x - c.x;
        float C_T1_Angle = Mathf.Atan2(C_T1_dy, C_T1_dx) * Mathf.Rad2Deg;
        Right.transform.localEulerAngles = new Vector3(0, 0, C_T1_Angle - (clipAngle + 90));

        NextPageClip.transform.localEulerAngles = new Vector3(0, 0, clipAngle + 90);
        NextPageClip.transform.position = BookPanel.TransformPoint(t1);
        RightNext.transform.SetParent(NextPageClip.transform, true);
        Left.transform.SetParent(ClippingPlane.transform, true);
        Left.transform.SetAsFirstSibling();

        Shadow.rectTransform.SetParent(Right.rectTransform, true);
    }
    private float CalcClipAngle(Vector3 c, Vector3 bookCorner, out Vector3 t1)
    {
        Vector3 t0 = (c + bookCorner) / 2;
        float T0_CORNER_dy = bookCorner.y - t0.y;
        float T0_CORNER_dx = bookCorner.x - t0.x;
        float T0_CORNER_Angle = Mathf.Atan2(T0_CORNER_dy, T0_CORNER_dx);
        float T0_T1_Angle = 90 - T0_CORNER_Angle;

        float T1_X = t0.x - T0_CORNER_dy * Mathf.Tan(T0_CORNER_Angle);
        T1_X = normalizeT1X(T1_X, bookCorner, sb);
        t1 = new Vector3(T1_X, sb.y, 0);

        //clipping plane angle=T0_T1_Angle
        float T0_T1_dy = t1.y - t0.y;
        float T0_T1_dx = t1.x - t0.x;
        T0_T1_Angle = Mathf.Atan2(T0_T1_dy, T0_T1_dx) * Mathf.Rad2Deg;
        return T0_T1_Angle;
    }
    private float normalizeT1X(float t1, Vector3 corner, Vector3 sb)
    {
        if (t1 > sb.x && sb.x > corner.x)
            return sb.x;
        if (t1 < sb.x && sb.x < corner.x)
            return sb.x;
        return t1;
    }
    private Vector3 Calc_C_Position(Vector3 followLocation)
    {
        Vector3 c;
        f = followLocation;
        float F_SB_dy = f.y - sb.y;
        float F_SB_dx = f.x - sb.x;
        float F_SB_Angle = Mathf.Atan2(F_SB_dy, F_SB_dx);
        Vector3 r1 = new Vector3(radius1 * Mathf.Cos(F_SB_Angle), radius1 * Mathf.Sin(F_SB_Angle), 0) + sb;

        float F_SB_distance = Vector2.Distance(f, sb);
        if (F_SB_distance < radius1)
            c = f;
        else
            c = r1;
        float F_ST_dy = c.y - st.y;
        float F_ST_dx = c.x - st.x;
        float F_ST_Angle = Mathf.Atan2(F_ST_dy, F_ST_dx);
        Vector3 r2 = new Vector3(radius2 * Mathf.Cos(F_ST_Angle),
           radius2 * Mathf.Sin(F_ST_Angle), 0) + st;
        float C_ST_distance = Vector2.Distance(c, st);
        if (C_ST_distance > radius2)
            c = r2;
        return c;
    }
    public void DragRightPageToPoint(Vector3 point)
    {
        if (currentPage >= bookPages.Length && flip10 == false && flip50 == false
            || currentPage + 9 >= bookPages.Length && flip10 == true
            || currentPage + 49 >= bookPages.Length && flip50 == true) return;
        pageDragging = true;
        mode = FlipMode.RightToLeft;
        f = point;


        NextPageClip.rectTransform.pivot = new Vector2(0, 0.12f);
        ClippingPlane.rectTransform.pivot = new Vector2(1, 0.35f);

        Left.gameObject.SetActive(true);
        Left.rectTransform.pivot = new Vector2(0, 0);
        Left.transform.position = RightNext.transform.position;
        Left.transform.eulerAngles = new Vector3(0, 0, 0);
        Left.sprite = (currentPage < bookPages.Length) ? bookPages[currentPage] : background;
        Left.transform.SetAsFirstSibling();

        Right.gameObject.SetActive(true);
        Right.transform.position = RightNext.transform.position;
        Right.transform.eulerAngles = new Vector3(0, 0, 0);
        if (flip10)
        {
            Right.sprite = (currentPage < bookPages.Length - 9) ? bookPages[currentPage + 9] : background;
            RightNext.sprite = (currentPage < bookPages.Length - 10) ? bookPages[currentPage + 10] : background;
        }
        else if (flip50)
        {
            Right.sprite = (currentPage < bookPages.Length - 49) ? bookPages[currentPage + 49] : background;
            RightNext.sprite = (currentPage < bookPages.Length - 50) ? bookPages[currentPage + 50] : background;
        }
        else
        {
            Right.sprite = (currentPage < bookPages.Length - 1) ? bookPages[currentPage + 1] : background;
            RightNext.sprite = (currentPage < bookPages.Length - 2) ? bookPages[currentPage + 2] : background;
        }

        LeftNext.transform.SetAsFirstSibling();
        if (enableShadowEffect) Shadow.gameObject.SetActive(true);
        UpdateBookRTLToPoint(f);
    }
    public void OnMouseDragRightPage()
    {
        // delay action to check if pinch zoom, and if not then perform drag
        touchTimer = Time.time;
        if (touchTimer + timerInterval >= Time.time)
        {
            if (interactable && Input.touchCount == 1)
                DragRightPageToPoint(transformPoint(Input.mousePosition));
        }

    }
    public void DragLeftPageToPoint(Vector3 point)
    {
        if (currentPage <= 0 && flip10 == false && flip50 == false
            || currentPage - 9 < 0 && flip10 == true
            || currentPage - 49 < 0 && flip50 == true) return;
        pageDragging = true;
        mode = FlipMode.LeftToRight;
        f = point;

        NextPageClip.rectTransform.pivot = new Vector2(1, 0.12f);
        ClippingPlane.rectTransform.pivot = new Vector2(0, 0.35f);

        Right.gameObject.SetActive(true);
        Right.transform.position = LeftNext.transform.position;
        Right.sprite = bookPages[currentPage - 1];
        Right.transform.eulerAngles = new Vector3(0, 0, 0);
        Right.transform.SetAsFirstSibling();

        Left.gameObject.SetActive(true);
        Left.rectTransform.pivot = new Vector2(1, 0);
        Left.transform.position = LeftNext.transform.position;
        Left.transform.eulerAngles = new Vector3(0, 0, 0);

        if (flip10)
        {
            Left.sprite = (currentPage >= 10) ? bookPages[currentPage - 10] : background;
            LeftNext.sprite = (currentPage >= 11) ? bookPages[currentPage - 11] : background;
        }
        else if (flip50)
        {
            Left.sprite = (currentPage >= 50) ? bookPages[currentPage - 50] : background;
            LeftNext.sprite = (currentPage >= 51) ? bookPages[currentPage - 51] : background;
        }
        else
        {
            Left.sprite = (currentPage >= 2) ? bookPages[currentPage - 2] : background;
            LeftNext.sprite = (currentPage >= 3) ? bookPages[currentPage - 3] : background;
        }

        RightNext.transform.SetAsFirstSibling();
        if (enableShadowEffect) ShadowLTR.gameObject.SetActive(true);
        UpdateBookLTRToPoint(f);
    }
    public void OnMouseDragLeftPage()
    {
        // delay action to check if pinch zoom, and if not then perform drag
        touchTimer = Time.time;
        if (touchTimer + timerInterval >= Time.time)
        {
            if (interactable && Input.touchCount == 1)
                DragLeftPageToPoint(transformPoint(Input.mousePosition));
        }

    }
    public void OnMouseRelease()
    {
        if (interactable)
            ReleasePage();
    }

    // function to deal with action after dragging
    public void ReleasePage()
    {
        if (pageDragging)
        {
            pageDragging = false;
            float distanceToLeft = Vector2.Distance(c, ebl);
            float distanceToRight = Vector2.Distance(c, ebr);
            if (distanceToRight < distanceToLeft / 8 && mode == FlipMode.RightToLeft)
                TweenBack();
            else if (distanceToRight / 8 > distanceToLeft && mode == FlipMode.LeftToRight)
                TweenBack();
            else
                TweenForward();
        }
    }
    Coroutine currentCoroutine;
    void UpdateSprites()
    {
        LeftNext.sprite = (currentPage > 0 && currentPage <= bookPages.Length) ? bookPages[currentPage - 1] : background;
        RightNext.sprite = (currentPage >= 0 && currentPage < bookPages.Length) ? bookPages[currentPage] : background;
    }
    public void TweenForward()
    {
        currentTime = countdownTime; // reset timer
        if (mode == FlipMode.RightToLeft)
            currentCoroutine = StartCoroutine(TweenTo(ebl, 0.15f, () => { Flip(); }));
        else
            currentCoroutine = StartCoroutine(TweenTo(ebr, 0.15f, () => { Flip(); }));

        // once done, enable the buttons
        btn_next.enabled = true;
        btn_prev.enabled = true;

    }
    void Flip()
    {
        currentTime = countdownTime;
        if (mode == FlipMode.RightToLeft)
        {
            if (flip10)
                currentPage += 10;
            else if (flip50)
                currentPage += 50;
            else
                currentPage += 2;

            updatePageNumber();
        }
        else
        {
            if (flip10)
                currentPage -= 10;
            else if (flip50)
                currentPage -= 50;
            else
                currentPage -= 2;

            updatePageNumber();
        }
        LeftNext.transform.SetParent(BookPanel.transform, true);
        Left.transform.SetParent(BookPanel.transform, true);
        LeftNext.transform.SetParent(BookPanel.transform, true);
        Left.gameObject.SetActive(false);
        Right.gameObject.SetActive(false);
        Right.transform.SetParent(BookPanel.transform, true);
        RightNext.transform.SetParent(BookPanel.transform, true);
        UpdateSprites();
        Shadow.gameObject.SetActive(false);
        ShadowLTR.gameObject.SetActive(false);
        if (OnFlip != null)
            OnFlip.Invoke();
    }

    public void OnScrollbarValueChange(float value)
    {
        // Calculate the total number of spreads.
        int totalSpreads = (TotalPageCount + 1) / 2; // Adding 1 to handle odd number of pages

        // Multiply by 2 to skip every other page.
        int targetSpread = Mathf.RoundToInt(value * (totalSpreads - 1));
        int targetPage = targetSpread * 2;

        // Ensure the target page does not exceed the total page count
        targetPage = Mathf.Min(targetPage, TotalPageCount - TotalPageCount % 2);

        if (currentPage != targetPage)
        {
            JumpToPage(targetPage);
        }
    }
    public void JumpToPage(int targetPage)
    {
        currentTime = countdownTime; // Reset timer
        currentPage = targetPage;
        UpdateSprites();
        // Reset any ongoing page dragging or animations if necessary
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        Left.gameObject.SetActive(false);
        Right.gameObject.SetActive(false);
        Shadow.gameObject.SetActive(false);
        ShadowLTR.gameObject.SetActive(false);
        UpdatePlayNameText();
    }

    // Separate jumpToPage for table of content that calls UpdateScrollbarPosition() for scrollbar
    public void GoToPage(int targetPage)
    {
        currentTime = countdownTime; // Reset timer
        currentPage = targetPage;
        UpdateSprites();
        // Reset any ongoing page dragging or animations if necessary
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        Left.gameObject.SetActive(false);
        Right.gameObject.SetActive(false);
        Shadow.gameObject.SetActive(false);
        ShadowLTR.gameObject.SetActive(false);
        UpdateScrollbarPosition(currentPage);
        UpdatePlayNameText();
    }

    public void UpdateScrollbarPosition(int currentPage)
    {
        // calculate the total number of spreads
        int totalSpreads = (TotalPageCount + 1) / 2; // Adjust for odd total page count

        int currentSpread = currentPage / 2;

        float scrollbarValue = (float)currentSpread / (totalSpreads - 1);
        scrollbarValue = Mathf.Clamp(scrollbarValue, 0f, 1f);

        // changethe scrollbar page handle to current page
        pageScrollBar.value = scrollbarValue;
    }
    public void TweenBack()
    {
        currentTime = countdownTime; // Reset timer
        if (mode == FlipMode.RightToLeft)
        {
            currentCoroutine = StartCoroutine(TweenTo(ebr, 0.15f,
                () =>
                {
                    UpdateSprites();
                    RightNext.transform.SetParent(BookPanel.transform);
                    Right.transform.SetParent(BookPanel.transform);

                    Left.gameObject.SetActive(false);
                    Right.gameObject.SetActive(false);
                    pageDragging = false;
                }
                ));
        }
        else
        {
            currentCoroutine = StartCoroutine(TweenTo(ebl, 0.15f,
                () =>
                {
                    UpdateSprites();

                    LeftNext.transform.SetParent(BookPanel.transform);
                    Left.transform.SetParent(BookPanel.transform);

                    Left.gameObject.SetActive(false);
                    Right.gameObject.SetActive(false);
                    pageDragging = false;
                }
                ));
        }

        // once done, enable the buttons
        btn_next.enabled = true;
        btn_prev.enabled = true;
        flip10 = false;
        flip50 = false;

    }
    public IEnumerator TweenTo(Vector3 to, float duration, System.Action onFinish)
    {
        int steps = (int)(duration / 0.025f);
        Vector3 displacement = (to - f) / steps;
        for (int i = 0; i < steps - 1; i++)
        {
            if (mode == FlipMode.RightToLeft)
                UpdateBookRTLToPoint(f + displacement);
            else
                UpdateBookLTRToPoint(f + displacement);

            yield return new WaitForSeconds(0.025f);
        }
        if (onFinish != null)
            onFinish();

        // once done, enable the buttons
        btn_next.enabled = true;
        btn_prev.enabled = true;
        flip10 = false;
        flip50 = false;

    }

    //function to start interaction when not zoomed and stop when there is zoom
    public void startInteraction()
    {
        if (content.transform.localScale.x == 1)
        {
            RightHotSpot.GetComponent<Image>().raycastTarget = true;
            LeftHotSpot.GetComponent<Image>().raycastTarget = true;
        }
        else
        {
            RightHotSpot.GetComponent<Image>().raycastTarget = false;
            LeftHotSpot.GetComponent<Image>().raycastTarget = false;
        }
    }
    public void quitGame()
    {
        Application.Quit();
    }
    private void updatePageNumber()
    {
        page_num.text = "Page " + currentPage;
    }
    public void setFlip10True()
    {
        flip10 = true;
    }
    public void setFlip50True()
    {
        flip50 = true;
    }
}
