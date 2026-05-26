using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    [UxmlElement]
    public partial class LoadingSpinner : VisualElement
    {
        [UxmlAttribute, CreateProperty] public float Speed = 300f;

        [UxmlAttribute, CreateProperty] public Color Color = Color.white;

        [UxmlAttribute, CreateProperty] public float Width = 5f;

        float _startAngle = 0f;
        long _lastTimestamp = -1;

        public LoadingSpinner()
        {
            style.width = 200;
            style.height = 200;

            generateVisualContent += Draw;

            // Schedule a repeating update every ~16ms (~60fps)
            schedule.Execute(Update).Every(16);
        }

        void Update(TimerState timeState)
        {
            if (_lastTimestamp < 0)
            {
                _lastTimestamp = timeState.start;
                return;
            }

            float deltaTime = (timeState.now - _lastTimestamp) / 1000f; // ms to seconds
            _lastTimestamp = timeState.now;

            _startAngle = (_startAngle + Speed * deltaTime) % 360f;

            MarkDirtyRepaint();
        }

        void Draw(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            var center = new Vector2(contentRect.width / 2, contentRect.height / 2);
            float radius = Mathf.Min(contentRect.width, contentRect.height) / 2 - 5f;

            painter.strokeColor = Color;
            painter.lineWidth = Width;
            painter.lineCap = LineCap.Round;

            painter.BeginPath();
            painter.Arc(center, radius, _startAngle, _startAngle + 270f); // 270° arc gap creates the spinner look
            painter.Stroke();
            painter.ClosePath();
        }
    }
}