using Microsoft.Band;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace HelloBand.BandTiles
{
	internal class GarageControlsLayout
	{
		private PageLayout pageLayout;
		private PageLayoutData pageLayoutData;
		
		private FlowPanel panel = new FlowPanel();
		private TextButton button = new TextButton();
		private TextButton button2 = new TextButton();
		
		private TextButtonData buttonData = new TextButtonData(2, "Open");
		private TextButtonData button2Data = new TextButtonData(3, "Close");
		
		public GarageControlsLayout()
		{
			LoadIconMethod = LoadIcon;
			AdjustUriMethod = (uri) => uri;
			
			panel = new FlowPanel();
			panel.Orientation = FlowPanelOrientation.Vertical;
			panel.Rect = new PageRect(0, 0, 248, 128);
			panel.ElementId = 1;
			panel.Margins = new Margins(0, 0, 0, 0);
			panel.HorizontalAlignment = HorizontalAlignment.Left;
			panel.VerticalAlignment = VerticalAlignment.Top;
			panel.Visible = true;
			
			button = new TextButton();
			button.PressedColor = new BandColor(32, 32, 32);
			button.Rect = new PageRect(0, 0, 90, 32);
			button.ElementId = 2;
			button.Margins = new Margins(10, 10, 10, 10);
			button.HorizontalAlignment = HorizontalAlignment.Center;
			button.VerticalAlignment = VerticalAlignment.Top;
			button.Visible = true;
			
			panel.Elements.Add(button);
			
			button2 = new TextButton();
			button2.PressedColor = new BandColor(32, 32, 32);
			button2.Rect = new PageRect(0, 0, 90, 32);
			button2.ElementId = 3;
			button2.Margins = new Margins(10, 10, 10, 10);
			button2.HorizontalAlignment = HorizontalAlignment.Center;
			button2.VerticalAlignment = VerticalAlignment.Top;
			button2.Visible = true;
			
			panel.Elements.Add(button2);
			pageLayout = new PageLayout(panel);
			
			PageElementData[] pageElementDataArray = new PageElementData[2];
			pageElementDataArray[0] = buttonData;
			pageElementDataArray[1] = button2Data;
			
			pageLayoutData = new PageLayoutData(pageElementDataArray);
		}
		
		public PageLayout Layout
		{
			get
			{
				return pageLayout;
			}
		}
		
		public PageLayoutData Data
		{
			get
			{
				return pageLayoutData;
			}
		}
		
		public Func<string, Task<BandIcon>> LoadIconMethod
		{
			get;
			set;
		}
		
		public Func<string, string> AdjustUriMethod
		{
			get;
			set;
		}
		
		private static async Task<BandIcon> LoadIcon(string uri)
		{
			StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
			
			using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
			{
				WriteableBitmap bitmap = new WriteableBitmap(1, 1);
				await bitmap.SetSourceAsync(fileStream);
				return bitmap.ToBandIcon();
			}
		}
		
		public async Task LoadIconsAsync(BandTile tile)
		{
			await Task.Run(() => { }); // Dealing with CS1998
		}
		
		public static BandTheme GetBandTheme()
		{
			var theme = new BandTheme();
			theme.Base = new BandColor(51, 102, 204);
			theme.HighContrast = new BandColor(58, 120, 221);
			theme.Highlight = new BandColor(58, 120, 221);
			theme.Lowlight = new BandColor(49, 101, 186);
			theme.Muted = new BandColor(43, 90, 165);
			theme.SecondaryText = new BandColor(137, 151, 171);
			return theme;
		}
		
		public static BandTheme GetTileTheme()
		{
			var theme = new BandTheme();
			theme.Base = new BandColor(51, 102, 204);
			theme.HighContrast = new BandColor(58, 120, 221);
			theme.Highlight = new BandColor(58, 120, 221);
			theme.Lowlight = new BandColor(49, 101, 186);
			theme.Muted = new BandColor(43, 90, 165);
			theme.SecondaryText = new BandColor(137, 151, 171);
			return theme;
		}
		
		public class PageLayoutData
		{
			private PageElementData[] array = new PageElementData[2];
			private PageElementData[] clone;
			
			public PageLayoutData(PageElementData[] pageElementDataArray)
			{
				array = pageElementDataArray;
			}
			
			public int Count
			{
				get
				{
					return array.Length;
				}
			}
			
			public T Get<T>(int i) where T : PageElementData
			{
				return (T)array[i];
			}
			
			public T ById<T>(short id) where T:PageElementData
			{
				return (T)array.FirstOrDefault(elm => elm.ElementId == id);
			}
			
			public PageElementData[] All
			{
				get
				{
					return (PageElementData[])array.Clone();
				}
			}
			
			public void BeginModify()
			{
				int length = array.Length;
				clone = new PageElementData[length];
				for (int i = 0; i < length; ++i)
				{
					clone[i] = DataUtils.Clone(array[i]);
				}
			}
			
			public PageElementData[] EndModify()
			{
				if (clone == null)
				{
					throw new InvalidOperationException("BeginModify was not called.");
				}
				
				int length = array.Length;
				var modified = new List<PageElementData>(length);
				for (int i = 0; i < length; ++i)
				{
					if (clone == null || !DataUtils.Same(array[i], clone[i]))
					{
						modified.Add(array[i]);
					}
				}
				clone = null;
				return modified.ToArray();
			}
		}
		
		class DataUtils
		{
			public static PageElementData Clone(PageElementData src)
			{
				var textD = src as TextBlockData;
				if (textD != null)
				{
					return new TextBlockData(textD.ElementId, textD.Text);
				}
				
				var buttonD = src as TextButtonData;
				if (buttonD != null)
				{
					return new TextButtonData(buttonD.ElementId, buttonD.Text);
				}
				
				var wrappedD = src as WrappedTextBlockData;
				if (wrappedD != null)
				{
					return new WrappedTextBlockData(wrappedD.ElementId, wrappedD.Text);
				}
				
				var iconD = src as IconData;
				if (iconD != null)
				{
					return new IconData(iconD.ElementId, iconD.IconIndex);
				}
				
				var filledButtonD = src as FilledButtonData;
				if (filledButtonD != null)
				{
					return new FilledButtonData(filledButtonD.ElementId, filledButtonD.PressedColor);
				}
				
				var barCodeD = src as BarcodeData;
				if (barCodeD != null)
				{
					return new BarcodeData(barCodeD.BarcodeType, barCodeD.ElementId, barCodeD.Barcode);
				}
				
				throw new NotImplementedException("Unrecognized type of PageElementData");
			}
			
			public static bool Same(PageElementData lhs, PageElementData rhs)
			{
				var textDL = lhs as TextData;
				if (textDL != null)
				{
					var textDR = rhs as TextData;
					return textDR != null && textDL.ElementId == textDR.ElementId && textDL.Text == textDR.Text;
				}
				
				var iconDL = lhs as IconData;
				if (iconDL != null)
				{
					var iconDR = rhs as IconData;
					return iconDR != null && iconDL.ElementId == iconDR.ElementId && iconDL.IconIndex == iconDR.IconIndex;
				}
				
				var filledButtonDL = lhs as FilledButtonData;
				if (filledButtonDL != null)
				{
					var filledButtonDR = rhs as FilledButtonData;
					return filledButtonDR != null && filledButtonDL.ElementId == filledButtonDR.ElementId && ValueType.Equals(filledButtonDL.PressedColor, filledButtonDR.PressedColor);
				}
				
				var barCodeDL = lhs as BarcodeData;
				if (barCodeDL != null)
				{
					var barCodeDR = rhs as BarcodeData;
					return barCodeDR != null && barCodeDL.BarcodeType == barCodeDR.BarcodeType && barCodeDL.ElementId == barCodeDR.ElementId && barCodeDL.Barcode == barCodeDR.Barcode;
				}
				
				throw new NotImplementedException("Unrecognized type of PageElementData");
			}
		}
	}
}
