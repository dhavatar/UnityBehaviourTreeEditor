using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheKiwiCoder
{
    public class FlowingEdge : Edge
    {
        public bool enableFlow
        {
            get => _isEnableFlow;
            set
            {
                if (_isEnableFlow == value)
                {
                    return;
                }

                _isEnableFlow = value;
                if (_isEnableFlow)
                {
                    edgeControl.inputColor = Color.green;
                    Add(_flowImg);
                }
                else
                {
                    edgeControl.inputColor = defaultColor;
                    Remove(_flowImg);
                }
            }
        }

        private bool _isEnableFlow;

        public float flowSize
        {
            get => _flowSize;
            set
            {
                _flowSize = value;
                _flowImg.style.width = new Length(_flowSize, LengthUnit.Pixel);
                _flowImg.style.height = new Length(_flowSize, LengthUnit.Pixel);
            }
        }

        private float _flowSize = 6f;

        public float flowSpeed { get; set; } = 150f;

        private readonly Image _flowImg;

        public FlowingEdge()
        {
            _flowImg = new Image
            {
                name = "flow-image",
                style =
                {
                    width = new Length(flowSize, LengthUnit.Pixel),
                    height = new Length(flowSize, LengthUnit.Pixel),
                    borderTopLeftRadius = new Length(flowSize / 2, LengthUnit.Pixel),
                    borderTopRightRadius = new Length(flowSize / 2, LengthUnit.Pixel),
                    borderBottomLeftRadius = new Length(flowSize / 2, LengthUnit.Pixel),
                    borderBottomRightRadius = new Length(flowSize / 2, LengthUnit.Pixel),
                }
            };
            // Add(_flowImg);

            edgeControl.RegisterCallback<GeometryChangedEvent>(OnEdgeControlGeometryChanged);
        }

        public override bool UpdateEdgeControl()
        {
            if (!base.UpdateEdgeControl())
            {
                return false;
            }

            UpdateFlow();
            return true;
        }


        #region Flow

        private float _totalEdgeLength;

        private float _passedEdgeLength;

        private int _flowPhaseIndex;

        private double _flowPhaseStartTime;

        private double _flowPhaseDuration;

        private float _currentPhaseLength;


        public void UpdateFlow()
        {
            if (!enableFlow)
            {
                return;
            }

            // Position
            var posProgress = (EditorApplication.timeSinceStartup - _flowPhaseStartTime) / _flowPhaseDuration;
            var flowStartPoint = edgeControl.controlPoints[_flowPhaseIndex];
            var flowEndPoint = edgeControl.controlPoints[_flowPhaseIndex + 1];
            var flowPos = Vector2.Lerp(flowStartPoint, flowEndPoint, (float)posProgress);
            _flowImg.transform.position = flowPos - Vector2.one * flowSize / 2;

            // Color
            var colorProgress = (_passedEdgeLength + _currentPhaseLength * posProgress) / _totalEdgeLength;
            var startColor = edgeControl.outputColor;
            var endColor = edgeControl.inputColor;
            var flowColor = Color.Lerp(startColor, endColor, (float)colorProgress);
            _flowImg.style.backgroundColor = flowColor;

            // Enter next phase
            if (posProgress >= 0.99999f)
            {
                _passedEdgeLength += _currentPhaseLength;

                _flowPhaseIndex++;
                if (_flowPhaseIndex >= edgeControl.controlPoints.Length - 1)
                {
                    // Restart flow
                    _flowPhaseIndex = 0;
                    _passedEdgeLength = 0;
                }

                _flowPhaseStartTime = EditorApplication.timeSinceStartup;
                _currentPhaseLength = Vector2.Distance(edgeControl.controlPoints[_flowPhaseIndex],
                    edgeControl.controlPoints[_flowPhaseIndex + 1]);
                _flowPhaseDuration = _currentPhaseLength / flowSpeed;
            }
        }

        private void OnEdgeControlGeometryChanged(GeometryChangedEvent evt)
        {
            // Restart flow
            _flowPhaseIndex = 0;
            _passedEdgeLength = 0;
            _flowPhaseStartTime = EditorApplication.timeSinceStartup;
            _currentPhaseLength = Vector2.Distance(edgeControl.controlPoints[_flowPhaseIndex],
                edgeControl.controlPoints[_flowPhaseIndex + 1]);
            _flowPhaseDuration = _currentPhaseLength / flowSpeed;

            // Calculate edge path length
            _totalEdgeLength = 0;
            for (int i = 0; i < edgeControl.controlPoints.Length - 1; i++)
            {
                var p = edgeControl.controlPoints[i];
                var pNext = edgeControl.controlPoints[i + 1];
                var phaseLen = Vector2.Distance(p, pNext);
                _totalEdgeLength += phaseLen;
            }
        }

        #endregion
    }
}