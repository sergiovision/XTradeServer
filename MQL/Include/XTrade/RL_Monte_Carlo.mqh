//+------------------------------------------------------------------+
//|                                               RL Monte Carlo.mqh |
//|                                 Copyright 2018, Max Dmitrievskiy |
//|                        https://www.mql5.com/ru/users/dmitrievsky |
//+------------------------------------------------------------------+
#property copyright "Copyright 2018, Max Dmitrievskiy"
#property link      "https://www.mql5.com/ru/users/dmitrievsky"
#include <Math\Alglib\dataanalysis.mqh>
#include <Math\Alglib\alglib.mqh> 
#include <XTrade\MT4Orders.mqh>
sinput int      OrderMagic=666;
//+------------------------------------------------------------------+
//|RL agent base class                                               |
//+------------------------------------------------------------------+
class CRLAgent
  {
public:
                     CRLAgent(string,int,int,int,double, double);
                    ~CRLAgent(void);
   static int        agentIDs;

   void              updatePolicy(double,double&[]);
   void              updateReward();
   double            getTradeSignal(double&[]);
   int               trees;
   double            r;
   int               features;
   double            rferrors[], lastrferrors[];
   string            Name;
   double            learnAnAgent();
   int               bestfeatures_num;
   double            prob_shift;

private:
   CMatrixDouble     RDFpolicyMatrix;
   CDecisionForest   RDF;
   CDFReport         RDF_report;

   double            RFout[];
   int               RDFinfo;
   int               agentID;
   int               numberOfsamples;
   void              getRDFstructure();
   double            getLastProfit();
   int               getLastOrderType();
   void              RecursiveElimination();
   double            bestFeatures[][2];
   bool random;
  };

static int CRLAgent::agentIDs=0;
//+------------------------------------------------------------------+
CRLAgent::CRLAgent(string AgentName,int number_of_features, int bestFeatures_number, int number_of_trees,double regularization, double shift_probability) {
   random=false;
   MathSrand(GetTickCount());
   ArrayResize(rferrors,2);
   ArrayResize(lastrferrors,2);
   Name = AgentName;
   ArrayResize(RFout,2);
   trees = number_of_trees;
   r = regularization;
   features = number_of_features;
   bestfeatures_num = bestFeatures_number;
   prob_shift = shift_probability;
   if(bestfeatures_num>features) bestfeatures_num = features;
   ArrayResize(bestFeatures,1);
   numberOfsamples = 0;
   agentIDs++;
   agentID = agentIDs;
   getRDFstructure();
  }
CRLAgent::~CRLAgent() {};
//+------------------------------------------------------------------+
//|Load learned agent                                                |
//+------------------------------------------------------------------+
void CRLAgent::getRDFstructure(void) {   
   string path=_Symbol+(string)_Period+Name+"\\";
   if(MQLInfoInteger(MQL_OPTIMIZATION)) {
    if(FileIsExist(path+"RFlasterrors"+(string)agentID+".rl",FILE_COMMON)) {
      int getRDF; 
      do {
        getRDF=FileOpen(path+"RFlasterrors"+(string)agentID+".rl",FILE_READ|FILE_BIN|FILE_ANSI|FILE_COMMON);
        FileReadArray(getRDF,lastrferrors,0);
        FileClose(getRDF);
        }
      while (getRDF<0);   
       }
     else {
       int getRDF;   
       do {
        getRDF=FileOpen(path+"RFlasterrors"+(string)agentID+".rl",FILE_WRITE|FILE_BIN|FILE_ANSI|FILE_COMMON);
        double arr[2];
        ArrayInitialize(arr,1);
        FileWriteArray(getRDF,arr,0);
        FileClose(getRDF);
        }
      while (getRDF<0);
      }
     return;
    }
    
   if(FileIsExist(path+"RFmodel"+(string)agentID+".rl",FILE_COMMON)) {
      int getRDF=FileOpen(path+"RFmodel"+(string)agentID+".rl",FILE_READ|FILE_TXT|FILE_COMMON);
      CSerializer serialize;
      string RDFmodel="";
      while(FileIsEnding(getRDF)==false)
         RDFmodel+=" "+FileReadString(getRDF);

      FileClose(getRDF);
      serialize.UStart_Str(RDFmodel);
      CDForest::DFUnserialize(serialize,RDF);
      serialize.Stop();

      getRDF=FileOpen(path+"Kernel"+(string)agentID+".rl",FILE_READ|FILE_BIN|FILE_ANSI|FILE_COMMON);
         FileReadArray(getRDF,bestFeatures,0);
         FileClose(getRDF);
      
      getRDF=FileOpen(path+"RFerrors"+(string)agentID+".rl",FILE_READ|FILE_BIN|FILE_ANSI|FILE_COMMON);
         FileReadArray(getRDF,rferrors,0);
         FileClose(getRDF);
      
      getRDF=FileOpen(path+"RFlasterrors"+(string)agentID+".rl",FILE_WRITE|FILE_BIN|FILE_ANSI|FILE_COMMON);
         double arr[2];
         ArrayInitialize(arr,1);
         FileWriteArray(getRDF,arr,0);
         FileClose(getRDF);
     }
    else random = true;
  }
//+------------------------------------------------------------------+
//|Recursive feature elimitation for matrix inputs                   |
//+------------------------------------------------------------------+
void CRLAgent::RecursiveElimination(void) {
//feature transformation, making every 2 features as returns with different lag's                            
   ArrayResize(bestFeatures,0);
   ArrayInitialize(bestFeatures,0);                 
   CDecisionForest   mRDF;                         
   CMatrixDouble     m;                             
   CDFReport         mRep;                          
   m.Resize(RDFpolicyMatrix.Size(),3);    
   int modelCounterInitial = 0;          
 
     for(int bf=1;bf<features;bf++) {               
      for(int i=0;i<RDFpolicyMatrix.Size();i++) {   
        m[i].Set(0,RDFpolicyMatrix[i][0]/RDFpolicyMatrix[i][bf]);         
        m[i].Set(1,RDFpolicyMatrix[i][features]);   
        m[i].Set(2,RDFpolicyMatrix[i][features+1]); 
       }         
      CDForest::DFBuildRandomDecisionForest(m,RDFpolicyMatrix.Size(),1,2,trees,r,RDFinfo,mRDF,mRep);         
      ArrayResize(bestFeatures,ArrayRange(bestFeatures,0)+1);
      bestFeatures[modelCounterInitial][0] = mRep.m_oobrelclserror;   
      bestFeatures[modelCounterInitial][1] = bf; 
      modelCounterInitial++;                     
     }  
     
  ArraySort(bestFeatures);                               
  ArrayResize(bestFeatures,bestfeatures_num);                 
  
  m.Resize(RDFpolicyMatrix.Size(),2+ArrayRange(bestFeatures,0));
   
  for(int i=0;i<RDFpolicyMatrix.Size();i++) { 
    for(int l=0;l<ArrayRange(bestFeatures,0);l++)
       m[i].Set(l,RDFpolicyMatrix[i][0]/RDFpolicyMatrix[i][(int)bestFeatures[l][1]]);           
    m[i].Set(ArrayRange(bestFeatures,0),RDFpolicyMatrix[i][features]);
    m[i].Set(ArrayRange(bestFeatures,0)+1,RDFpolicyMatrix[i][features+1]);
   }
      
  CDForest::DFBuildRandomDecisionForest(m,RDFpolicyMatrix.Size(),ArrayRange(bestFeatures,0),2,trees,r,RDFinfo,RDF,RDF_report);
 }
//+------------------------------------------------------------------+
//|Learn an agent                                                    |
//+------------------------------------------------------------------+
double CRLAgent::learnAnAgent(void)
  {
   if(MQLInfoInteger(MQL_OPTIMIZATION)) {
      if(numberOfsamples>0) {
         RecursiveElimination();    
         if(RDF_report.m_oobrelclserror<lastrferrors[1]) { 
          string path=_Symbol+(string)_Period+Name+"\\";

          CSerializer serialize;
          serialize.Alloc_Start();
          CDForest::DFAlloc(serialize,RDF);
          serialize.SStart_Str();
          CDForest::DFSerialize(serialize,RDF);
          serialize.Stop();

          int setRDF;
          
          do {
           setRDF=FileOpen(path+"RFlasterrors"+(string)agentID+".rl",FILE_WRITE|FILE_BIN|FILE_ANSI|FILE_COMMON);
           if(setRDF<0) continue;
           lastrferrors[0]=RDF_report.m_relclserror;
           lastrferrors[1]=RDF_report.m_oobrelclserror;
           FileWriteArray(setRDF,lastrferrors,0);
           FileClose(setRDF); 
          
           setRDF=FileOpen(path+"RFmodel"+(string)agentID+".rl",FILE_WRITE|FILE_TXT|FILE_COMMON);
           FileWrite(setRDF,serialize.Get_String());
           FileClose(setRDF);
        
           setRDF=FileOpen(path+"RFerrors"+(string)agentID+".rl",FILE_WRITE|FILE_BIN|FILE_ANSI|FILE_COMMON);
           rferrors[0]=RDF_report.m_relclserror;
           rferrors[1]=RDF_report.m_oobrelclserror;
           FileWriteArray(setRDF,rferrors,0);
           FileClose(setRDF);
          
           setRDF=FileOpen(path+"Kernel"+(string)agentID+".rl",FILE_WRITE|FILE_BIN|FILE_ANSI|FILE_COMMON);
           FileWriteArray(setRDF,bestFeatures);
           FileClose(setRDF); 
          }
          while(setRDF<0);        
         }
        }
     }
   return 1-RDF_report.m_oobrelclserror;
  }
//+------------------------------------------------------------------+
//|Update agent policy                                               |
//+------------------------------------------------------------------+
void CRLAgent::updatePolicy(double action,double &featuresValues[]) {
   if(MQLInfoInteger(MQL_OPTIMIZATION)) {
      numberOfsamples++;
      RDFpolicyMatrix.Resize(numberOfsamples,features+2);
      //input variables
      for(int i=0;i<features;i++)
         RDFpolicyMatrix[numberOfsamples-1].Set(i,featuresValues[i]);
      //output variables
      RDFpolicyMatrix[numberOfsamples-1].Set(features,action);
      RDFpolicyMatrix[numberOfsamples-1].Set(features+1,1-action);
     }
  }
//+------------------------------------------------------------------+
//|Update agent reward                                               |
//+------------------------------------------------------------------+
void CRLAgent::updateReward(void) {
   if(MQLInfoInteger(MQL_OPTIMIZATION)) {
      if(getLastProfit()>0) return;
      double randomReward = (getLastOrderType()==0) ? 1 : 0;
      RDFpolicyMatrix[numberOfsamples-1].Set(features,randomReward);
      RDFpolicyMatrix[numberOfsamples-1].Set(features+1,1-randomReward);
     }
  }
//+------------------------------------------------------------------+
//|Take last state profit                                            |
//+------------------------------------------------------------------+
double CRLAgent::getLastProfit() {
   int result=0;
   long lasttime=0;
   double lasprofit=0;
   for(int k=0; k<OrdersHistoryTotal(); k++)
      if(OrderSelect(k,SELECT_BY_POS,MODE_HISTORY)==true) {
         if(OrderOpenTime()>lasttime) {
            lasprofit=OrderProfit();
            lasttime = OrderOpenTime();
           }
        }
   return(lasprofit);
  }
//+------------------------------------------------------------------+
//|Take last state order type                                        |
//+------------------------------------------------------------------+
int CRLAgent::getLastOrderType() {
   int result=0;
   long lasttime=0;
   int type=0;
   for(int k=0; k<OrdersHistoryTotal(); k++)
      if(OrderSelect(k,SELECT_BY_POS,MODE_HISTORY)==true) {
         if(OrderOpenTime()>lasttime) {
            type=OrderType();
            lasttime=OrderOpenTime();
           }
        }
   return(type);
  }
//+------------------------------------------------------------------+
//|Get trade signal                                                  |
//+------------------------------------------------------------------+
double CRLAgent::getTradeSignal(double &featuresValues[]) {
   double res=0.5;
   if(!MQLInfoInteger(MQL_OPTIMIZATION) && !random) {
      double kerfeatures[];
      ArrayResize(kerfeatures,ArrayRange(bestFeatures,0));
      ArrayInitialize(kerfeatures,0);
      
      for(int i=0;i<ArraySize(kerfeatures);i++) {
         kerfeatures[i] = featuresValues[0]/featuresValues[(int)bestFeatures[i][1]];
        }
            
      CDForest::DFProcess(RDF,kerfeatures,RFout);
      return RFout[1];
     }
   else {
     if(countOrders()==0) if(rand()/32767.0<0.5) res = 0; else res = 1;
     else {
      if(countOrders(0)!=0) if(rand()/32767.0>prob_shift) res = 0; else res = 1;
      if(countOrders(1)!=0) if(rand()/32767.0<prob_shift) res = 0; else res = 1;
     }
    }  
   return res;
  }
//+------------------------------------------------------------------+
//|Multiple RL agents class                                          |
//+------------------------------------------------------------------+
class CRLAgents
  {
private:
   struct Agents
     {
      double            inpVector[];
      CRLAgent         *ag;
      double            rms;
      double            oob;
     };

   void              getStatistics();
   string            groupName;
public:
                     CRLAgents(string,int,int,int,int,double,double);
                    ~CRLAgents(void);
   Agents            agent[];
   void              updatePolicies(double);
   void              updateRewards();
   double            getTradeSignal();
   double            learnAllAgents();
   void              setAgentSettings(int,int,int,int,double,double);
  };
//+------------------------------------------------------------------+
CRLAgents::CRLAgents(string AgentsName,int agentsQuantity,int features, int bestfeatures, int treesNumber,double regularization, double shift_probability)
  {
   groupName=AgentsName;
   ArrayResize(agent,agentsQuantity);
   for(int i=0;i<agentsQuantity;i++) {
      ArrayResize(agent[i].inpVector,features);
      ArrayInitialize(agent[i].inpVector,0);
      agent[i].ag  = new CRLAgent(AgentsName, features, bestfeatures, treesNumber, regularization, shift_probability);
      agent[i].rms = agent[i].ag.rferrors[0];
      agent[i].oob = agent[i].ag.rferrors[1];
     }
  }
//+------------------------------------------------------------------+
//|Learn all agents                                                  |
//+------------------------------------------------------------------+
double CRLAgents::learnAllAgents(void){
   double err=0;
   for(int i=0;i<ArraySize(agent);i++)
      err+=agent[i].ag.learnAnAgent();
  return err/ArraySize(agent);
 }
//+------------------------------------------------------------------+
//|Destructor learn and delete all agents                            |
//+------------------------------------------------------------------+
CRLAgents::~CRLAgents() {
   getStatistics();
   for(int i=0;i<ArraySize(agent);i++)
      delete agent[i].ag;
  }
//+------------------------------------------------------------------+
//|Change agents settings                                            |
//+------------------------------------------------------------------+
void CRLAgents::setAgentSettings(int agentNumber,int features,int bestfeatures,int treesNumber,double regularization,double shift_probability) {
   agent[agentNumber].ag.features=features;
   agent[agentNumber].ag.bestfeatures_num=bestfeatures;
   agent[agentNumber].ag.trees=treesNumber;
   agent[agentNumber].ag.r=regularization;
   agent[agentNumber].ag.prob_shift=shift_probability;
   ArrayResize(agent[agentNumber].inpVector,features);
   ArrayInitialize(agent[agentNumber].inpVector,0);
  }
//+------------------------------------------------------------------+
//|Update policies for all agents                                    |
//+------------------------------------------------------------------+
void CRLAgents::updatePolicies(double action) {
   for(int i=0;i<ArraySize(agent);i++)
      agent[i].ag.updatePolicy(action,agent[i].inpVector);
  }
//+------------------------------------------------------------------+
//|Update rewards for all agents                                     |
//+------------------------------------------------------------------+
void CRLAgents::updateRewards(void) {
   for(int i=0;i<ArraySize(agent);i++)
      agent[i].ag.updateReward();
  }
//+------------------------------------------------------------------+
//|Get common trade signal                                           |
//+------------------------------------------------------------------+
double CRLAgents::getTradeSignal() {
   double signal[];
   double sig=0;
   ArrayResize(signal,ArraySize(agent));

   for(int i=0;i<ArraySize(agent);i++)
      sig+=signal[i]=agent[i].ag.getTradeSignal(agent[i].inpVector);
   return sig/(double)ArraySize(agent);
  }
//+------------------------------------------------------------------+
//|Get agents statistics                                             |
//+------------------------------------------------------------------+
void CRLAgents::getStatistics(void)
  {
   double arr[];
   double arrrms[];
   ArrayResize(arr,ArraySize(agent));
   ArrayResize(arrrms,ArraySize(agent));

   for(int i=0;i<ArraySize(agent);i++) {
      arrrms[i]=agent[i].rms;
      arr[i]=agent[i].oob;
     }

   Print(groupName+" TRAIN LOSS");
   ArrayPrint(arrrms);
   Print(groupName+" OOB LOSS");
   ArrayPrint(arr);
  }
//+------------------------------------------------------------------+
int countOrders() {
   int result=0;
   for(int k=0; k<OrdersTotal(); k++) {
      if(OrderSelect(k,SELECT_BY_POS,MODE_TRADES)==true)
         if(OrderMagicNumber()==OrderMagic)
            result++;
     }
   return(result);
  }

int countOrders(int a) {
   int result=0;
   for(int k=0; k<OrdersTotal(); k++) {
      if(OrderSelect(k,SELECT_BY_POS,MODE_TRADES)==true)
         if(OrderType()==a && OrderMagicNumber()==OrderMagic)
            result++;
     }
   return(result);
  }