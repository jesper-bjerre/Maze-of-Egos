using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace MazeOfEgos.Runtime
{
    public sealed class TouchControls : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public bool joystick;
        public RectTransform knob;
        public CanvasGroup artwork;
        public float travel = 76;
        public Vector2 Movement { get; private set; }
        private int pointer = int.MinValue;
        public void OnPointerDown(PointerEventData e)
        {
            if (pointer != int.MinValue) return;
            pointer = e.pointerId;
            if (joystick) ReadJoystick(e);
        }
        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != pointer) return;
            if (joystick) ReadJoystick(e);
        }
        public void OnPointerUp(PointerEventData e) { if (e.pointerId == pointer) Clear(); }
        private void ReadJoystick(PointerEventData e)
        {
            RectTransform rect = (RectTransform)transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out Vector2 local);
            float radius = Mathf.Max(1, travel);
            Vector2 raw = Vector2.ClampMagnitude(local / radius, 1);
            Movement = raw.magnitude < 0.12f ? Vector2.zero : raw.normalized * ((raw.magnitude - 0.12f) / 0.88f);
            if (knob) knob.anchoredPosition = raw * radius;
        }
        public void Clear()
        {
            pointer = int.MinValue; Movement = Vector2.zero;
            if (knob) knob.anchoredPosition = Vector2.zero;
        }
        private void OnDisable() => Clear();
        private void Update()
        {
            if (artwork) artwork.alpha = Mathf.MoveTowards(artwork.alpha, pointer == int.MinValue ? 0.42f : 0.95f, Time.unscaledDeltaTime * 5);
        }
    }

    public sealed class DesktopActions : System.IDisposable
    {
        public readonly InputAction Move, Use, Pause;
        public DesktopActions()
        {
            Move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            Move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            Move.AddBinding("<Gamepad>/leftStick");
            Move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            Use = new InputAction("Use item", InputActionType.Button);
            Use.AddBinding("<Keyboard>/space"); Use.AddBinding("<Gamepad>/buttonSouth");
            Pause = new InputAction("Pause", InputActionType.Button);
            Pause.AddBinding("<Keyboard>/escape"); Pause.AddBinding("<Gamepad>/start");
            Move.Enable(); Use.Enable(); Pause.Enable();
        }
        public void Dispose() { Move.Dispose(); Use.Dispose(); Pause.Dispose(); }
    }
}
