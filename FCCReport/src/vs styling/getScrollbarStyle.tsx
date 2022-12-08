import {CSSProperties, SVGAttributes} from "react"

type BackgroundColor = CSSProperties['backgroundColor'];
type BorderColor = CSSProperties['borderColor'];
type SvgFill = SVGAttributes<SVGSVGElement>['fill']

/* forcedColorAdjust:"none" as when vs allows high contrast EnvironmentColors has the high contrast colors*/
export function getScrollbarStyle(
  thumbColor: BackgroundColor,
  thumbHoverColor: BackgroundColor,
  thumbActiveColor: BackgroundColor,
  trackColor: BackgroundColor,
  arrowBackgroundColor: BackgroundColor,
  arrowBackgroundHoverColor: BackgroundColor,
  arrowBackgroundActiveColor: BackgroundColor,
  arrowGlyphBackgroundColor: SvgFill,
  arrowGlyphBackgroundHoverColor: SvgFill,
  arrowGlyphBackgroundActiveColor: SvgFill,
  scrollBarBorderColor:  BorderColor,
  scrollBarThumbBorderColor: BorderColor,
  scrollBarThumbBorderHoverColor: BorderColor,
  scrollBarThumbBorderActiveColor: BorderColor,
  arrowBorderHoverColor: BorderColor,
  arrowBorderActiveColor: BorderColor,
  scrollbarSize = 18,
  thumbSize = 8,
) {
  const hide = (scrollbarSize - thumbSize)/2;
  const arrowShift = (scrollbarSize - thumbSize/2)/2;

  function getArrow(points: string, fill: SvgFill) {
    return `url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100' fill='${fill}'><polygon points='${points}'/></svg>")`;
  }

  function getArrowStyles(isHorizontal: boolean) {
    const vOrH = isHorizontal ? "horizontal" : "vertical";
    const points: [string, string] = isHorizontal ? ["0,50 50,100 50,0", "0,0 0,100 50,50"] : ["50,00 0,50 100,50", "0,0 100,0 50,50"];
    return {
      // left or up
      //
      [`::-webkit-scrollbar-button:single-button:${vOrH}:decrement`]: {
        backgroundPosition: isHorizontal ? `${arrowShift}px center` : `center ${arrowShift}px`,
        backgroundImage: getArrow(points[0], arrowGlyphBackgroundColor)
      },
      [`::-webkit-scrollbar-button:single-button:${vOrH}:decrement:hover`]: {
        backgroundImage: getArrow(points[0], arrowGlyphBackgroundHoverColor)
      },
      [`::-webkit-scrollbar-button:single-button:vertical:decrement:active`]: {
        backgroundImage: getArrow(points[0], arrowGlyphBackgroundActiveColor)
      },

      // right or down
      [`::-webkit-scrollbar-button:single-button:${vOrH}:increment`]: {
        
        backgroundPosition: isHorizontal ? `${arrowShift}px center` : `center ${arrowShift}px`,
        backgroundImage: getArrow(points[1], arrowGlyphBackgroundColor)
      },
      [`::-webkit-scrollbar-button:single-button:${vOrH}:increment:hover`]: {
        backgroundImage: getArrow(points[1], arrowGlyphBackgroundHoverColor)
      },
      [`::-webkit-scrollbar-button:single-button:${vOrH}:increment:active`]: {
        backgroundImage: getArrow(points[1], arrowGlyphBackgroundActiveColor)
      }
    };
  }

  function getBorder(borderColor: BorderColor) {
    return {};
    /* return {
      // box shadow ?
      border:`1px solid ${borderColor}`
    } */
  }


  return {
    "::-webkit-scrollbar": {
      width: `${scrollbarSize}px`,
      height: `${scrollbarSize}px`,
    },

    "::-webkit-scrollbar-corner": {
      forcedColorAdjust:"none",
      backgroundColor: trackColor,
      ...getBorder(scrollBarBorderColor)
    },
    // the track (progress bar) of the scrollbar, where there is a gray bar on top of a white bar
    "::-webkit-scrollbar-track": {
      backgroundColor: trackColor,
      forcedColorAdjust:"none",
      ...getBorder(scrollBarBorderColor)
    },


    // what press to slide
    "::-webkit-scrollbar-thumb": {
      backgroundColor: thumbColor, // necessary even when styling differently
      forcedColorAdjust:"none",
    },
    "::-webkit-scrollbar-thumb:vertical": {
      //...getBorder(scrollBarThumbBorderColor)
      borderLeft: `${hide}px solid ${trackColor}`,
      borderRight: `${hide}px solid ${trackColor}`,
      backgroundClip: "content-box",
    },
    "::-webkit-scrollbar-thumb:horizontal": {
      //...getBorder(scrollBarThumbBorderColor)
      borderTop: `${hide}px solid ${trackColor}`,
      borderBottom: `${hide}px solid ${trackColor}`,
      backgroundClip: "content-box",
    },
    "::-webkit-scrollbar-thumb:hover": {
      backgroundColor: thumbHoverColor,
      //...getBorder(scrollBarThumbBorderHoverColor)
    },
    "::-webkit-scrollbar-thumb:active": {
      backgroundColor: thumbActiveColor,
      //...getBorder(scrollBarThumbBorderActiveColor)
    },

    //the buttons on the scrollbar (arrows pointing upwards and downwards that scroll one line at a time
    "::-webkit-scrollbar-button:single-button": {
      height:`${scrollbarSize}px`,
      width:`${scrollbarSize}px`,
      backgroundColor: arrowBackgroundColor,
      ...getBorder(scrollBarBorderColor),
      display: 'block',
      backgroundSize: `${thumbSize}px`,
      backgroundRepeat: 'no-repeat',
      forcedColorAdjust:"none",
    },
    "::-webkit-scrollbar-button:single-button:hover": {
      backgroundColor: arrowBackgroundHoverColor,
      ...getBorder(arrowBorderHoverColor)
    },
    "::-webkit-scrollbar-button:single-button:active": {
      backgroundColor: arrowBackgroundActiveColor,
      ...getBorder(arrowBorderActiveColor)
    },
    ...getArrowStyles(true),
    ...getArrowStyles(false)
  };
}
