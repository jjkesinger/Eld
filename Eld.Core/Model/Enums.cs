
namespace Eld.Core.Model
{
    public class Enums
    {
        public enum DutyStatus
        {
            OffDuty,
            Sleeper,
            Driving,
            OnDutyNotDriving
        }
        public enum ViolationType
        {
            OnDuty,
            Rest,
            Driving,
            Weekly
        }
        public enum Jurisdiction
        {
            TxIntrastate,
            Us70,
            Us60,
            TxInterstateOilfield,
            TxConstruction,
            TxConstruction70_7,
            TxConstruction70_8,
            Passenger70,
            Passenger60,
            Florida,
            FlInterstate,
            Ca70,
            Ca120,
            Alaska,
            UsOilfield70,
            UsOilfield60,
            Us70E,
            FlIntrastate
        }
    }
}
