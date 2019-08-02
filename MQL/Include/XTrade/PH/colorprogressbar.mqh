//+------------------------------------------------------------------+
//|                                             ColorProgressBar.mqh |
//|                        Copyright 2012, MetaQuotes Software Corp. |
//|                                              http://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Copyright 2012, MetaQuotes Software Corp."
#property link      "http://www.mql5.com"
#property version   "1.00"

#include <Canvas\Canvas.mqh>
//+------------------------------------------------------------------+
//|  Класс прогресс-бара, рисующий двумя цветами                     |
//+------------------------------------------------------------------+
class CColorProgressBar :public CCanvas
  {
private:
   color             m_goodcolor,m_badcolor;    // "хороший" и "плохой" цвета
   color             m_backcolor,m_bordercolor; // цвета фона и рамки
   int               m_x;                       // X координата левого верхнего угла 
   int               m_y;                       // Y координата левого верхнего угла 
   int               m_width;                   // ширина
   int               m_height;                  // высота
   int               m_borderwidth;             // толщина рамки
   bool              m_passes[];                // количество обработанных проходов
   int               m_lastindex;               // номер последнего прохода
public:
   //--- конструктор/деструктор
                     CColorProgressBar();
                    ~CColorProgressBar(){ CCanvas::Destroy(); };
   //--- инициализация
   bool              Create(const string name,int x,int y,int width,int height,ENUM_COLOR_FORMAT clrfmt);
   //--- сбрасывает счетчик в ноль
   void              Reset(void)                 { m_lastindex=0;     };
   //--- цвет фона, рамки и линии
   void              BackColor(const color clr)  { m_backcolor=clr;   };
   void              BorderColor(const color clr){ m_bordercolor=clr; };
   //---             переводит представление цвета из типа color в тип uint
   uint              uCLR(const color clr)          { return(XRGB((clr)&0x0FF,(clr)>>8,(clr)>>16));};
   //--- толщина рамки и линии
   void              BorderWidth(const int w) { m_borderwidth=w;      };
   //--- добавим результат для отрисовки полоски в прогресс-баре
   void              AddResult(bool good);
   //--- обновление прогресс-бара на графике
   void              Update(void);
  };
//+------------------------------------------------------------------+
//| Конструктор                                                      |
//+------------------------------------------------------------------+
CColorProgressBar::CColorProgressBar():m_lastindex(0),m_goodcolor(clrSeaGreen),m_badcolor(clrLightPink)
  {
//--- зазадим размер массива проходов с запасом
   ArrayResize(m_passes,5000,1000);
   ArrayInitialize(m_passes,0);
//---
  }
//+------------------------------------------------------------------+
//|  Инициализация                                                   |
//+------------------------------------------------------------------+
bool CColorProgressBar::Create(const string name,int x,int y,int width,int height,ENUM_COLOR_FORMAT clrfmt)
  {
   bool res=false;
//--- вызываем родительский класс для создания холста
   if(CCanvas::CreateBitmapLabel(name,x,y,width,height,clrfmt))
     {
      //--- запомним ширину и высоту
      m_height=height;
      m_width=width;
      res=true;
     }
//--- результат
   return(res);
  }
//+------------------------------------------------------------------+
//|  Добавление результата                                           |
//+------------------------------------------------------------------+
void CColorProgressBar::AddResult(bool good)
  {
   m_passes[m_lastindex]=good;
//--- добавим еще одну вертикальную черту нужного цвета в прогресс-бара
   LineVertical(m_lastindex,m_borderwidth,m_height-m_borderwidth,uCLR(good?m_goodcolor:m_badcolor));
//--- обновление на графике
   CCanvas::Update();
//--- обновление индекса
   m_lastindex++;
   if(m_lastindex>=m_width) m_lastindex=0;
//---
  }
//+------------------------------------------------------------------+
//|  Обновление чарта                                                |
//+------------------------------------------------------------------+
void CColorProgressBar::Update(void)
  {
//--- зальем цветом рамки фон
   CCanvas::Erase(CColorProgressBar::uCLR(m_bordercolor));
//--- нарисуем прямоугольник цветом фона
   CCanvas::FillRectangle(m_borderwidth,m_borderwidth,
                           m_width-m_borderwidth-1,
                           m_height-m_borderwidth-1,
                           CColorProgressBar::uCLR(m_backcolor));
//--- обновим чарт
   CCanvas::Update();
  }
//+------------------------------------------------------------------+
