using Cairo;

namespace Pinta.Core;

public sealed class BrushStrokeArgs
{
	public Color StrokeColor { get; }
	public PointI CurrentPosition { get; }
	public PointI LastPosition { get; }
	public bool UseAntialiasing { get; }

	public BrushStrokeArgs (
		Color strokeColor,
		PointI currentPosition,
		PointI lastPosition,
		bool useAntialiasing
	)
	{
		StrokeColor = strokeColor;
		CurrentPosition = currentPosition;
		LastPosition = lastPosition;
		UseAntialiasing = useAntialiasing;
	}
}