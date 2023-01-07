﻿using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Terminal.Gui;
using Terminal.Gui.Graphs;

namespace UICatalog.Scenarios {

	[ScenarioMetadata (Name: "Drawing", Description: "Demonstrates StraightLineCanvas.")]
	[ScenarioCategory ("Controls")]
	[ScenarioCategory ("Layout")]
	public class Drawing : Scenario {

		public override void Setup ()
		{
			var toolsWidth = 8;

			var canvas = new DrawingArea {
				Width = Dim.Fill (-toolsWidth),
				Height = Dim.Fill ()
			};

			var tools = new ToolsView (toolsWidth) {
				Y = 1,
				X = Pos.AnchorEnd (toolsWidth+1),
				Height = Dim.Fill (),
				Width = Dim.Fill ()				
			};


			tools.ColorChanged += (c) => canvas.SetColor (c);

			Win.Add(canvas);
			Win.Add (tools);
			Win.Add (new Label (" -Tools-") { X = Pos.AnchorEnd (toolsWidth + 1) });
		}

		class ToolsView : View {

			StraightLineCanvas grid;
			public event Action<Color> ColorChanged;

			Dictionary<Point, Color> swatches = new Dictionary<Point, Color> {
				{ new Point(1,1),Color.Red},
				{ new Point(3,1),Color.Green},
				{ new Point(5,1),Color.BrightBlue},
				{ new Point(7,1),Color.Black},
				{ new Point(1,3),Color.White},
			};

			public ToolsView (int width)
			{
				grid = new StraightLineCanvas (Application.Driver);

				grid.AddLine (new Point (0, 0), int.MaxValue, Orientation.Vertical, BorderStyle.Single);
				grid.AddLine (new Point (0, 0), width, Orientation.Horizontal, BorderStyle.Single);
				grid.AddLine (new Point (width, 0), int.MaxValue, Orientation.Vertical, BorderStyle.Single);

				grid.AddLine (new Point (0, 2), width, Orientation.Horizontal, BorderStyle.Single);

				grid.AddLine (new Point (2, 0), int.MaxValue, Orientation.Vertical, BorderStyle.Single);
				grid.AddLine (new Point (4, 0), int.MaxValue, Orientation.Vertical, BorderStyle.Single);
				grid.AddLine (new Point (6, 0), int.MaxValue, Orientation.Vertical, BorderStyle.Single);


				grid.AddLine (new Point (0, 4), width, Orientation.Horizontal, BorderStyle.Single);
			}
			public override void Redraw (Rect bounds)
			{
				base.Redraw (bounds);

				Driver.SetAttribute (new Terminal.Gui.Attribute (Color.Gray, ColorScheme.Normal.Background));
				grid.Draw (this, bounds);

				foreach(var swatch in swatches) {
					Driver.SetAttribute (new Terminal.Gui.Attribute (swatch.Value, ColorScheme.Normal.Background));
					AddRune (swatch.Key.X, swatch.Key.Y, '█');
				}
			}

			public override bool OnMouseEvent (MouseEvent mouseEvent)
			{
				if (mouseEvent.Flags.HasFlag (MouseFlags.Button1Clicked)) {
					foreach (var swatch in swatches) {
						if(mouseEvent.X == swatch.Key.X && mouseEvent.Y == swatch.Key.Y) {

							ColorChanged?.Invoke (swatch.Value);
							return true;
						}
					}
				}

				return base.OnMouseEvent (mouseEvent);
			}
		}

		class DrawingArea : View
		{
			/// <summary>
			/// Index into <see cref="canvases"/> by color.
			/// </summary>
			Dictionary<Color, int> colorLayers = new Dictionary<Color, int> ();
			List<StraightLineCanvas> canvases = new List<StraightLineCanvas>();
			int currentColor;

			Point? currentLineStart = null;

			public DrawingArea ()
			{
				AddCanvas (Color.White);
			}

			private void AddCanvas (Color c)
			{
				if(colorLayers.ContainsKey(c)) {
					return;
				}

				canvases.Add (new StraightLineCanvas (Application.Driver));
				colorLayers.Add (c, canvases.Count-1);
				currentColor = canvases.Count - 1;
			}

			public override void Redraw (Rect bounds)
			{
				base.Redraw (bounds);
				
				foreach(var kvp in colorLayers) {

					Driver.SetAttribute (new Terminal.Gui.Attribute (kvp.Key, ColorScheme.Normal.Background));
					canvases [kvp.Value].Draw (this, bounds);
				}
			}
			public override bool OnMouseEvent (MouseEvent mouseEvent)
			{

				if(mouseEvent.Flags.HasFlag(MouseFlags.Button1Pressed)) {
					if(currentLineStart == null) {
						currentLineStart = new Point(mouseEvent.X,mouseEvent.Y);
					}
				}
				else {
					if (currentLineStart != null) {

						var start = currentLineStart.Value;
						var end = new Point(mouseEvent.X, mouseEvent.Y);
						var orientation = Orientation.Vertical;
						var length = end.Y - start.Y;

						// if line is wider than it is tall switch to horizontal
						if(Math.Abs(start.X - end.X) > Math.Abs(start.Y - end.Y)) 
						{
							orientation = Orientation.Horizontal;
							length = end.X - start.X;
						}


						canvases [currentColor].AddLine (start, length, orientation, BorderStyle.Single);
						currentLineStart = null;
						SetNeedsDisplay ();
					}
				}

				return base.OnMouseEvent (mouseEvent);
			}

			internal void SetColor (Color c)
			{
				AddCanvas (c);
				currentColor = colorLayers [c];
			}
		}
	}
}
