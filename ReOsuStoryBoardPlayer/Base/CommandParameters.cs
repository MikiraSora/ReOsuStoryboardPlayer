using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public abstract class CommandParameters{}

    public class MoveXCommandParameters : CommandParameters
    {
        private MoveXCommandParameters() { }
        public MoveXCommandParameters(float startX, float endX)
        {
            StartX = startX;
            EndX = endX;
            Distance = EndX - StartX;
        }

        public readonly float Distance;
        public float StartX, EndX;

        public override string ToString() => $"{StartX},{EndX}";
    }

    public class MoveYCommandParameters : CommandParameters
    {
        private MoveYCommandParameters() { }
        public MoveYCommandParameters(float startY, float endY)
        {
            StartY = startY;
            EndY = endY;
            Distance = EndY - StartY;
        }

        public readonly float Distance;
        public float StartY, EndY;

        public override string ToString() => $"{StartY},{EndY}";
    }

    public class ColorCommandParameters : CommandParameters
    {
        private ColorCommandParameters() { }
        public ColorCommandParameters(Vec4 startColor, Vec4 endColor)
        {
            StartColor = startColor;
            EndColor = endColor;
            Distance = new Vec4(EndColor.x-StartColor.x, EndColor.y - StartColor.y, EndColor.z - StartColor.z, EndColor.w - StartColor.w);
        }

        public readonly Vec4 Distance;
        public Vec4 StartColor, EndColor;
        
        public override string ToString() => $"{StartColor},{EndColor}";
    }

    public class MoveCommandParameters : CommandParameters
    {
        private MoveCommandParameters() { }
        public MoveCommandParameters(Vector startPosition,Vector endPosition)
        {
            StartPostion = startPosition;
            EndPosition = endPosition;
            Distance = EndPosition - StartPostion;
        }

        public readonly Vector Distance;
        public Vector StartPostion, EndPosition;

        public override string ToString() => $"{StartPostion},{EndPosition}";
    }

    public class ScaleCommandParameters : CommandParameters
    {
        private ScaleCommandParameters() { }
        public ScaleCommandParameters(float startScale,float endScale)
        {
            StartScale = startScale;
            EndScale = endScale;
            Distance = EndScale - StartScale;
        }

        public readonly float Distance;
        public float StartScale, EndScale;
        
        public override string ToString() => $"{StartScale},{EndScale}";
    }

    public class ScaleVectorCommandParamesters : CommandParameters
    {
        private ScaleVectorCommandParamesters() { }
        public ScaleVectorCommandParamesters(Vector startScale,Vector endScale)
        {
            StartScale = startScale;
            EndScale = endScale;
            Distance = EndScale - StartScale;
        }

        public readonly Vector Distance;
        public Vector StartScale, EndScale;
        
        public override string ToString() => $"{StartScale.ToString()},{EndScale.ToString()}";
    }

    public class FadeCommandParamesters : CommandParameters
    {
        private FadeCommandParamesters() { }
        public FadeCommandParamesters(float startFade,float endFade)
        {
            StartFade = startFade;
            EndFade = endFade;
            Distance = EndFade - StartFade;
        }

        public readonly float Distance;
        public float StartFade, EndFade;

        public override string ToString() => $"{StartFade},{EndFade}";
    }

    public class RotateCommandParamesters : CommandParameters
    {
        private RotateCommandParamesters() { }
        public RotateCommandParamesters(float startRotate, float endRotate)
        {
            StartRotate = startRotate;
            EndRotate = endRotate;
            Distance = endRotate - StartRotate;
        }

        public readonly float Distance;
        public float StartRotate, EndRotate;
        
        public override string ToString() => $"{StartRotate},{EndRotate}";
    }

    public class LoopCommandParamesters : CommandParameters
    {
        public List<Command> LoopCommandList = new List<Command>();

        public LoopCommandParamesters() { }

        public uint CostTime;
    }

    public class ParameterCommandParamester : CommandParameters
    {
        public EffectParameter Effect;

        public ParameterCommandParamester(EffectParameter effect)
        {
            Effect = effect;
        }

        public override string ToString() => $"{Effect.ToString()}";
    }
}
