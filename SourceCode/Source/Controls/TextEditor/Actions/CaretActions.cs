﻿/*********************************************        
作者：曹旭升              
QQ：279060597
访问博客了解详细介绍及更多内容：   
http://blog.shengxunwei.com
**********************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using Sheng.SailingEase.Controls.TextEditor.Document;
namespace Sheng.SailingEase.Controls.TextEditor.Actions
{
	public class CaretLeft : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			TextLocation position = textArea.Caret.Position;
			List<FoldMarker> foldings = textArea.Document.FoldingManager.GetFoldedFoldingsWithEnd(position.Y);
			FoldMarker justBeforeCaret = null;
			foreach (FoldMarker fm in foldings) {
				if (fm.EndColumn == position.X) {
					justBeforeCaret = fm;
					break; 
				}
			}
			if (justBeforeCaret != null) {
				position.Y = justBeforeCaret.StartLine;
				position.X = justBeforeCaret.StartColumn;
			} else {
				if (position.X > 0) {
					--position.X;
				} else if (position.Y  > 0) {
					LineSegment lineAbove = textArea.Document.GetLineSegment(position.Y - 1);
					position = new TextLocation(lineAbove.Length, position.Y - 1);
				}
			}
			textArea.Caret.Position = position;
			textArea.SetDesiredColumn();
		}
	}
	public class CaretRight : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			LineSegment curLine = textArea.Document.GetLineSegment(textArea.Caret.Line);
			TextLocation position = textArea.Caret.Position;
			List<FoldMarker> foldings = textArea.Document.FoldingManager.GetFoldedFoldingsWithStart(position.Y);
			FoldMarker justBehindCaret = null;
			foreach (FoldMarker fm in foldings) {
				if (fm.StartColumn == position.X) {
					justBehindCaret = fm;
					break;
				}
			}
			if (justBehindCaret != null) {
				position.Y = justBehindCaret.EndLine;
				position.X = justBehindCaret.EndColumn;
			} else { 
				if (position.X < curLine.Length || textArea.TextEditorProperties.AllowCaretBeyondEOL) {
					++position.X;
				} else if (position.Y + 1 < textArea.Document.TotalNumberOfLines) {
					++position.Y;
					position.X = 0;
				}
			}
			textArea.Caret.Position = position;
			textArea.SetDesiredColumn();
		}
	}
	public class CaretUp : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			TextLocation position = textArea.Caret.Position;
			int lineNr = position.Y;
			int visualLine = textArea.Document.GetVisibleLine(lineNr);
			if (visualLine > 0) {
				Point pos = new Point(textArea.TextView.GetDrawingXPos(lineNr, position.X),
				                      textArea.TextView.DrawingPosition.Y + (visualLine - 1) * textArea.TextView.FontHeight - textArea.TextView.TextArea.VirtualTop.Y);
				textArea.Caret.Position = textArea.TextView.GetLogicalPosition(pos);
				textArea.SetCaretToDesiredColumn();
			}
		}
	}
	public class CaretDown : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			TextLocation position = textArea.Caret.Position;
			int lineNr = position.Y;
			int visualLine = textArea.Document.GetVisibleLine(lineNr);
			if (visualLine < textArea.Document.GetVisibleLine(textArea.Document.TotalNumberOfLines)) {
				Point pos = new Point(textArea.TextView.GetDrawingXPos(lineNr, position.X),
				                      textArea.TextView.DrawingPosition.Y
				                      + (visualLine + 1) * textArea.TextView.FontHeight
				                      - textArea.TextView.TextArea.VirtualTop.Y);
				textArea.Caret.Position = textArea.TextView.GetLogicalPosition(pos);
				textArea.SetCaretToDesiredColumn();
			}
		}
	}
	public class WordRight : CaretRight
	{
		public override void Execute(TextArea textArea)
		{
			LineSegment line   = textArea.Document.GetLineSegment(textArea.Caret.Position.Y);
			TextLocation oldPos = textArea.Caret.Position;
			TextLocation newPos;
			if (textArea.Caret.Column >= line.Length) {
				newPos = new TextLocation(0, textArea.Caret.Line + 1);
			} else {
				int nextWordStart = TextUtilities.FindNextWordStart(textArea.Document, textArea.Caret.Offset);
				newPos = textArea.Document.OffsetToPosition(nextWordStart);
			}
			List<FoldMarker> foldings = textArea.Document.FoldingManager.GetFoldingsFromPosition(newPos.Y, newPos.X);
			foreach (FoldMarker marker in foldings) {
				if (marker.IsFolded) {
					if (oldPos.X == marker.StartColumn && oldPos.Y == marker.StartLine) {
						newPos = new TextLocation(marker.EndColumn, marker.EndLine);
					} else {
						newPos = new TextLocation(marker.StartColumn, marker.StartLine);
					}
					break;
				}
			}
			textArea.Caret.Position = newPos;
			textArea.SetDesiredColumn();
		}
	}
	public class WordLeft : CaretLeft
	{
		public override void Execute(TextArea textArea)
		{
			TextLocation oldPos = textArea.Caret.Position;
			if (textArea.Caret.Column == 0) {
				base.Execute(textArea);
			} else {
				LineSegment line   = textArea.Document.GetLineSegment(textArea.Caret.Position.Y);
				int prevWordStart = TextUtilities.FindPrevWordStart(textArea.Document, textArea.Caret.Offset);
				TextLocation newPos = textArea.Document.OffsetToPosition(prevWordStart);
				List<FoldMarker> foldings = textArea.Document.FoldingManager.GetFoldingsFromPosition(newPos.Y, newPos.X);
				foreach (FoldMarker marker in foldings) {
					if (marker.IsFolded) {
						if (oldPos.X == marker.EndColumn && oldPos.Y == marker.EndLine) {
							newPos = new TextLocation(marker.StartColumn, marker.StartLine);
						} else {
							newPos = new TextLocation(marker.EndColumn, marker.EndLine);
						}
						break;
					}
				}
				textArea.Caret.Position = newPos;
				textArea.SetDesiredColumn();
			}
		}
	}
	public class ScrollLineUp : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			textArea.AutoClearSelection = false;
			textArea.MotherTextAreaControl.VScrollBar.Value = Math.Max(textArea.MotherTextAreaControl.VScrollBar.Minimum,
			                                                           textArea.VirtualTop.Y - textArea.TextView.FontHeight);
		}
	}
	public class ScrollLineDown : AbstractEditAction
	{
		public override void Execute(TextArea textArea)
		{
			textArea.AutoClearSelection = false;
			textArea.MotherTextAreaControl.VScrollBar.Value = Math.Min(textArea.MotherTextAreaControl.VScrollBar.Maximum,
			                                                           textArea.VirtualTop.Y + textArea.TextView.FontHeight);
		}
	}
}