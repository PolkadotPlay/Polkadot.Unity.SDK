using Assets.Scripts.ScreenStates;
using Substrate.Hexalem.Engine;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public class GridManager : Singleton<GridManager>
    {
        private GameObject _playerGrid;

        public delegate void TileClickHandler(GameObject tileObject, int index);

        public event TileClickHandler OnGridTileClicked;

        private bool isZoomed = false;
        private Vector3 originCameraPosition;
        private Vector3 originCameraAngle;

        public delegate void SwipeHandler(Vector3 direction, bool isOverUI);

        public event SwipeHandler OnSwipeEvent;

        [SerializeField]
        public GameObject PlayerGrid;

        private Vector2 touchStart;

        private Vector2 touchEnd;

        private bool isSwiping;

        public float swipeThreshold = 50f; // Minimum distance for a swipe

        public float cameraMoveSpeed = 1f; // Speed of camera movement

        private bool isPointerOverUI = false;

        private VisualElement _root;

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            PlayScreenState.ZoomClicked += OnZoomClicked;
            originCameraPosition = Camera.main.transform.position;
            originCameraAngle = Camera.main.transform.eulerAngles;

            Debug.Log($"[Zoom] Camera initial position = {originCameraPosition} / initial angle = {originCameraAngle}");
        }

        public void RegisterBottomBound()
        {
            _root.Q("BottomBound").RegisterCallback<PointerEnterEvent>(e => isPointerOverUI = true);
            _root.Q("FloatBody").RegisterCallback<PointerEnterEvent>(e => isPointerOverUI = false);
        }

        private void Update()
        {
            // Check for touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                HandleInput(touch.phase, touch.position);
            }
            // Check for mouse input
            else if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
            {
                TouchPhase phase = Input.GetMouseButtonDown(0) ? TouchPhase.Began :
                                  (Input.GetMouseButtonUp(0) ? TouchPhase.Ended : TouchPhase.Moved);
                Vector2 position = Input.mousePosition;
                HandleInput(phase, position);
            }
        }

        private void HandleInput(TouchPhase phase, Vector2 position)
        {
            switch (phase)
            {
                case TouchPhase.Began:
                    touchStart = position;
                    isSwiping = false;
                    break;

                case TouchPhase.Moved:
                    touchEnd = position;
                    if (!isSwiping && Vector2.Distance(touchStart, touchEnd) >= swipeThreshold)
                    {
                        ProcessSwipe(touchEnd.x - touchStart.x, touchEnd.y - touchStart.y, isPointerOverUI);
                        isSwiping = true;
                    }
                    break;

                case TouchPhase.Ended:
                    if (!isSwiping)
                    {
                        ProcessTap(position);
                    }
                    break;
            }
        }

        public void OnZoomClicked()
        {
            var target = PlayerGrid.transform.GetChild(12).gameObject;
            if (!isZoomed)
            {
                ZoomFit(Camera.main, target, false);
            }
            else
            {
                Camera.main.transform.position = originCameraPosition;
                Camera.main.transform.eulerAngles = originCameraAngle;
                isZoomed = false;
            }
        }

        #region Zoom

        internal Bounds GetBound(GameObject go)
        {
            Bounds b = new Bounds(go.transform.position, Vector3.zero);
            var rList = go.GetComponentsInChildren(typeof(Renderer));
            foreach (Renderer r in rList)
            {
                b.Encapsulate(r.bounds);
            }
            return b;
        }

        /// <summary>
        /// Adjust the camera to zoom fit the game object
        /// There are multiple directions to get zoom-fit view of the game object,
        /// if ViewFromRandomDirecion is true, then random viewing direction is chosen
        /// else, the camera's forward direction will be sused
        /// </summary>
        /// <param name="c"> The camera, whose position and view direction will be
        //                   adjusted to implement zoom-fit effect </param>
        /// <param name="go"> The GameObject which will be zoom-fit. This object may have
        ///                   children objects as well </param>
        /// <param name="ViewFromRandomDirection"> if random viewing direction is chozen. </param>
        internal void ZoomFit(Camera c, GameObject go, bool ViewFromRandomDirection = false)
        {
            Bounds b = GetBound(go);
            Vector3 max = b.size;
            float radius = Mathf.Max(max.x, Mathf.Max(max.y, max.z));
            float dist = radius / Mathf.Sin(c.fieldOfView * Mathf.Deg2Rad / 2f);
            Debug.Log("Radius = " + radius + " dist = " + dist);

            Vector3 view_direction = ViewFromRandomDirection ? UnityEngine.Random.onUnitSphere : c.transform.InverseTransformDirection(Vector3.forward);

            Vector3 pos = view_direction * dist + b.center;
            c.transform.position = pos;
            c.transform.LookAt(b.center);

            Debug.Log($"[Zoom] New camera position = {c.transform.position} / initial angle = {c.transform.eulerAngles}");
            isZoomed = true;
        }

        #endregion Zoom

        #region Map swipe

        private void ProcessSwipe(float xDist, float yDist, bool isOverUI)
        {
            if (Mathf.Abs(xDist) > Mathf.Abs(yDist))
            {
                if (xDist > 0)
                {
                    OnSwipeEvent?.Invoke(Vector3.left, isOverUI);
                }
                else
                {
                    OnSwipeEvent?.Invoke(Vector3.right, isOverUI);
                }
            }
            else
            {
                if (yDist > 0)
                {
                    OnSwipeEvent?.Invoke(Vector3.down, isOverUI);
                }
                else
                {
                    OnSwipeEvent?.Invoke(Vector3.up, isOverUI);
                }
            }
        }

        public void MoveCamera(Vector3 direction)
        {
            Camera.main.transform.Translate(direction * 1);
        }

        private void ProcessTap(Vector2 screenPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && !isPointerOverUI && hit.transform.parent != null && hit.transform.parent.name.StartsWith('t'))
            {
                var tileObject = hit.transform.gameObject;
                var index = int.Parse(tileObject.transform.parent.name[1..]);
                Debug.Log($"Tapped on {tileObject.name} [{index}]");
                OnGridTileClicked?.Invoke(tileObject, index);
            }
        }

        #endregion Map swipe

        public void CreateGrid(HexaBoard hexaBoard)
        {
            for (int i = 0; i < hexaBoard.Value.Length; i++)
            {
                HexaTile tile = hexaBoard.Value[i];

                var gridParent = PlayerGrid.transform.GetChild(i);

                // remove previous game objects connected to the grid
                foreach (Transform child in gridParent.transform)
                {
                    Destroy(child.gameObject);
                }

                var gameObjectTile = TileCreator.GetInstance().CreateTile(tile.TileType, tile.TileLevel, tile.TilePattern, gridParent);
            }
        }
    }
}