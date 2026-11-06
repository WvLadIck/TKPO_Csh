// SRP означает, что класс имеет одну связную ответственность и одну причину для изменения. 

// OCP означает возможность расширить поведение программы, сохранив существующий общий код.

namespace Lab1_SRP_OCP
{
    class Pacient
    {
        public required string Name { get; set; }
        public required Doctor AttenddingDoctor { get; set; }

        public bool isDischarged { get; private set; } = false;
        public string DischargeReason { get; private set; } = "";
        public void Discharge(string reason)
        {
            isDischarged = true;
            DischargeReason = reason;
        }

    }
    class Doctor
    {
        public required string Name { get; set; }
        public void Prescribe(Pacient pacient, Appointment appointment)
        {
            if (pacient.AttenddingDoctor != this)
            {
                throw new InvalidOperationException("Назначение может дать только лечащий врач");
            }
            appointment.pacient = pacient;
        }
        public void Perform(Appointment appointment)
        {
            if (appointment.pacient == null)
            {
                throw new InvalidOperationException(
                    "Назначение ещё не связано с пациентом.");
            }

            if (appointment.pacient.AttenddingDoctor == this)
            {
                throw new InvalidOperationException(
                    "Назначение должен выполнить другой врач.");
            }

            if (appointment.isComplete)
            {
                throw new InvalidOperationException(
                    "Назначение уже выполнено.");
            }

            appointment.Execute();
        }
    }
    class Nurse
    {
        public required string Name { get; set; }
        public void Perform(Appointment appointment)
        {
            if (appointment.pacient == null)
            {
                throw new InvalidOperationException(
                    "Назначение ещё не связано с пациентом.");
            }

            if (appointment.isComplete)
            {
                throw new InvalidOperationException(
                    "Назначение уже выполнено.");
            }

            appointment.Execute();
        }

    }
    abstract class Appointment
    {
        public Pacient? pacient { get; set; }
        public required string Description { get; set; }
        public bool isComplete { get; protected set; } = false;
        public abstract void Execute();

    }
    class Procedure : Appointment
    {
        public override void Execute()
        {
            isComplete = true;
        }
    }
    class Medication : Appointment
    {
        public override void Execute()
        {
            isComplete = true;
        }
    }
    class Surgery : Appointment
    {
        public override void Execute()
        {
            isComplete = true;
        }
    }
    internal class Program
    {
        static int passed;
        static int failed;
        static void Check(string name, bool condition)
        {
            if (condition)
            {
                passed++;
                Console.WriteLine($"[OK] {name}");
            }
            else
            {
                failed++;
                Console.WriteLine($"[ОШИБКА] {name}");
            }
        }

        static void ExpectError(string name, Action action, string expectedMessage)
        {
            try
            {
                action();
                Check(name, false); 
            }
            catch (InvalidOperationException exception)
            {
                Check(name, exception.Message == expectedMessage);
                Console.WriteLine($"  Причина отказа: {exception.Message}");
            }
        }
        static void Main(string[] args)
        {
            var doctor = new Doctor { Name = "Иван Петрович" };
            var otherDoctor = new Doctor { Name = "Алексей Сергеевич" };
            var nurse = new Nurse { Name = "Анна" };

            var pacient = new Pacient
            {
                Name = "Паша",
                AttenddingDoctor = doctor
            };

            var medication = new Medication
            {
                Description = "Приём лекарства"
            };

            doctor.Prescribe(pacient, medication);
            Check("Назначение связано с нужным пациентом",
                medication.pacient == pacient);

            nurse.Perform(medication);
            Console.WriteLine($"Назначение «{medication.Description}» выполнено.");
            Check("Медсестра выполнила назначение", medication.isComplete);

            Console.WriteLine(
                $"Лекарство выдано: {medication.isComplete}");

            var procedure = new Procedure
            {
                Description = "Лечебная процедура"
            };

            doctor.Prescribe(pacient, procedure);

            otherDoctor.Perform(procedure);
            Console.WriteLine($"Назначение «{procedure.Description}» выполнено.");
            Check("Другой врач выполнил назначение", procedure.isComplete);

            Console.WriteLine(
                $"Процедура выполнена: {procedure.isComplete}");

            var surgery = new Surgery { Description = "Операция" };
            doctor.Prescribe(pacient, surgery);
            otherDoctor.Perform(surgery);
            Check("Операция выполнена", surgery.isComplete);

            var unassigned = new Medication { Description = "Новое лекарство" };
            ExpectError("Чужой врач не может назначить лечение",
                () => otherDoctor.Prescribe(pacient, unassigned),
                "Назначение может дать только лечащий врач");
            Check("После отказа назначение осталось без пациента",
                unassigned.pacient == null);

            ExpectError("Медсестра не может выполнить назначение без пациента",
                () => nurse.Perform(unassigned),
                "Назначение ещё не связано с пациентом.");
            ExpectError("Врач не может выполнить назначение без пациента",
                () => otherDoctor.Perform(unassigned),
                "Назначение ещё не связано с пациентом.");
            Check("После отказов назначение не выполнено", !unassigned.isComplete);

            doctor.Prescribe(pacient, unassigned);
            ExpectError("Лечащий врач не может выполнить своё назначение",
                () => doctor.Perform(unassigned),
                "Назначение должен выполнить другой врач.");
            Check("После отказа лечащему врачу назначение не выполнено",
                !unassigned.isComplete);

            ExpectError("Медсестра не может повторно выполнить назначение",
                () => nurse.Perform(medication),
                "Назначение уже выполнено.");
            ExpectError("Врач не может повторно выполнить назначение",
                () => otherDoctor.Perform(procedure),
                "Назначение уже выполнено.");

        }

    }
}
