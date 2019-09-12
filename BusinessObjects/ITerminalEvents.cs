using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public interface ITerminalEvents
    {
        void InsertPosition(PositionInfo pos);
        void UpdatePosition(PositionInfo pos);
        void RemovePosition(long Ticket);

        void UpdatePositions(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos);

        void UpdateSLTP(long magicId, long AccountNumber, IEnumerable<PositionInfo> pos);
        // methods and functions
        List<PositionInfo> GetAllPositions();
        void DeletePosition(long Ticket);
        List<DealInfo> GetTodayDeals();

        TodayStat GetTodayStat();
        bool CheckTradeAllowed(SignalInfo signal);

        void UpdateBalance(int TerminalId, decimal Balance, decimal Equity);
    }
}