using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecoveryREST.Models.Classes;

namespace RecoveryREST.Data {
    public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<AppUser>(options) {
        public DbSet<RecoveryExercise> RecoveryExercises { get; set; }
        public DbSet<Injury> Injuries { get; set; }
        public DbSet<InjuryRecoveryExercise> InjuriesRecoveryExercises { get; set; }
        public DbSet<UserInjury> UserInjuries { get; set; }
        public DbSet<RecoveryPlan> RecoveryPlans { get; set; }
        public DbSet<WorkoutDay> WorkoutDays { get; set; }
        public DbSet<RecoveryPlanExercise> RecoveryPlanExercises { get; set; }
        public DbSet<UserInjuryHistory> UserInjuryHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
            
            builder.Entity<RecoveryPlan>()
                .HasOne(rp => rp.AppUser)
                .WithMany(au => au.RecoveryPlans)
                .HasForeignKey(rp => rp.AppUserId);

            builder.Entity<UserInjuryHistory>()
                .HasOne(uh => uh.AppUser)
                .WithMany()
                .HasForeignKey(uh => uh.AppUserId);

            builder.Entity<UserInjuryHistory>()
                .HasOne(uh => uh.Injury)
                .WithMany()
                .HasForeignKey(uh => uh.InjuryId);

            builder.Entity<WorkoutDay>()
                .HasOne(wd => wd.RecoveryPlan)
                .WithMany(rp => rp.WorkoutDays)
                .HasForeignKey(wd => wd.RecoveryPlanId);

            builder.Entity<RecoveryPlanExercise>()
                .HasOne(rpe => rpe.WorkoutDay)
                .WithMany(wd => wd.RecoveryPlanExercises)
                .HasForeignKey(rpe => rpe.WorkoutDayId);

            builder.Entity<RecoveryPlanExercise>()
                .HasOne(rpe => rpe.RecoveryExercise)
                .WithMany(re => re.RecoveryPlanExercises)
                .HasForeignKey(rpe => rpe.RecoveryExerciseId);

            builder.Entity<RecoveryPlanExercise>().HasKey(rpe => new { rpe.WorkoutDayId, rpe.RecoveryExerciseId });

            builder.Entity<InjuryRecoveryExercise>().HasKey(ire => new { ire.InjuryId, ire.RecoveryExerciseId });

            builder.Entity<InjuryRecoveryExercise>()
                .HasOne(ire => ire.Injury)
                .WithMany(i => i.InjuryRecoveryExercises)
                .HasForeignKey(ire => ire.InjuryId);

            builder.Entity<InjuryRecoveryExercise>()
                .HasOne(ire => ire.RecoveryExercise)
                .WithMany(re => re.InjuryRecoveryExercises)
                .HasForeignKey(ire => ire.RecoveryExerciseId);

            builder.Entity<UserInjury>().HasKey(ui => new { ui.AppUserId, ui.InjuryId });

            builder.Entity<UserInjury>()
                .HasOne(ui => ui.AppUser)
                .WithMany(u => u.UserInjuries)
                .HasForeignKey(ui => ui.AppUserId);

            builder.Entity<UserInjury>()
                .HasOne(ui => ui.Injury)
                .WithMany(i => i.UserInjuries)
                .HasForeignKey(ui => ui.InjuryId);

            builder.Entity<RecoveryExercise>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<Injury>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<WorkoutDay>().Property(x => x.Id).ValueGeneratedOnAdd(); 

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Name = "User", NormalizedName = "USER" },
                new IdentityRole { Name = "Doctor", NormalizedName = "DOCTOR" }
            );

            builder.Entity<RecoveryExercise>().HasData(
                new RecoveryExercise { Id = 1, Name = "Hamstring Stretch", Description = "A stretch to relieve tension in the hamstrings." },
                new RecoveryExercise { Id = 2, Name = "Quadriceps Stretch", Description = "A stretch targeting the quadriceps muscles." },
                new RecoveryExercise { Id = 3, Name = "Calf Raises", Description = "Strengthens the calf muscles and improves flexibility." },
                new RecoveryExercise { Id = 4, Name = "Plantar Fascia Stretch", Description = "Stretching the plantar fascia to relieve foot pain." },
                new RecoveryExercise { Id = 5, Name = "Ankle Proprioception Exercises", Description = "Exercises to improve ankle stability and proprioception." },
                new RecoveryExercise { Id = 6, Name = "Shin Splint Relief", Description = "Targeted movements to alleviate shin splints." },
                new RecoveryExercise { Id = 7, Name = "ACL Strengthening", Description = "Strengthening exercises for the anterior cruciate ligament." },
                new RecoveryExercise { Id = 8, Name = "MCL Rehabilitation Stretch", Description = "Stretch targeting the medial collateral ligament." },
                new RecoveryExercise { Id = 9, Name = "Shoulder Pendulum Exercises", Description = "Gentle swinging motion to improve shoulder mobility." },
                new RecoveryExercise { Id = 10, Name = "Rotator Cuff Strengthening", Description = "Strength exercises for the rotator cuff muscles." },
                new RecoveryExercise { Id = 11, Name = "Frozen Shoulder Stretch", Description = "Stretch to improve range of motion for frozen shoulder." },
                new RecoveryExercise { Id = 12, Name = "Wrist Flexor Stretch", Description = "Stretch to reduce tension in the wrist flexor muscles." },
                new RecoveryExercise { Id = 13, Name = "Groin Stretch", Description = "Relieves tension in the groin area." },
                new RecoveryExercise { Id = 14, Name = "Patellar Tendon Exercises", Description = "Exercises to strengthen the patellar tendon." },
                new RecoveryExercise { Id = 15, Name = "Dislocated Shoulder Recovery", Description = "Movements to regain shoulder stability." },
                new RecoveryExercise { Id = 16, Name = "Hip Flexor Stretch", Description = "Stretch to reduce tension in the hip flexors." },
                new RecoveryExercise { Id = 17, Name = "Neck Mobility Exercises", Description = "Exercises to improve neck flexibility." },
                new RecoveryExercise { Id = 18, Name = "Ankle Dorsiflexion Strengthening", Description = "Exercises to strengthen the ankle's dorsiflexion movement." },
                new RecoveryExercise { Id = 19, Name = "Achilles Tendon Stretch", Description = "Stretch to improve Achilles tendon flexibility." },
                new RecoveryExercise { Id = 20, Name = "Lower Back Extension", Description = "Strengthens the lower back muscles." },
                new RecoveryExercise { Id = 21, Name = "Glute Bridge", Description = "Strengthens glute muscles and reduces lower back pain." },
                new RecoveryExercise { Id = 22, Name = "Hip Abduction Exercise", Description = "Targets the hip abductors to improve mobility." },
                new RecoveryExercise { Id = 23, Name = "IT Band Foam Rolling", Description = "Relieves tension in the iliotibial band." },
                new RecoveryExercise { Id = 24, Name = "Cervical Retraction", Description = "Improves neck posture and reduces pain." },
                new RecoveryExercise { Id = 25, Name = "Knee Wall Slides", Description = "Strengthens the quadriceps while reducing knee strain." },
                new RecoveryExercise { Id = 26, Name = "Toe Spreading", Description = "Strengthens the foot's intrinsic muscles." },
                new RecoveryExercise { Id = 27, Name = "Tennis Elbow Isometric Holds", Description = "Builds strength in the elbow with minimal strain." },
                new RecoveryExercise { Id = 28, Name = "Lat Stretch", Description = "Improves flexibility in the latissimus dorsi muscles." },
                new RecoveryExercise { Id = 29, Name = "Side Plank", Description = "Strengthens the core and stabilizes the hip." },
                new RecoveryExercise { Id = 30, Name = "Finger Flexion Strengthening", Description = "Targets finger muscles for improved grip strength." },
                new RecoveryExercise { Id = 31, Name = "Seated Spinal Twist", Description = "Improves spinal flexibility and relieves tension." },
                new RecoveryExercise { Id = 32, Name = "Heel Drops", Description = "Strengthens the calf and Achilles tendon." },
                new RecoveryExercise { Id = 33, Name = "Scapular Retraction", Description = "Improves posture and shoulder strength." },
                new RecoveryExercise { Id = 34, Name = "Core Stability Drills", Description = "Enhances core strength for better balance." },
                new RecoveryExercise { Id = 35, Name = "Dynamic Wrist Rotations", Description = "Improves wrist flexibility and strength." },
                new RecoveryExercise { Id = 36, Name = "Shoulder Blade Squeeze", Description = "Improves upper back posture and relieves tension." },
                new RecoveryExercise { Id = 37, Name = "Cat-Cow Stretch", Description = "Promotes spinal flexibility and relieves back tension." },
                new RecoveryExercise { Id = 38, Name = "Hip Flexor Activation", Description = "Strengthens hip flexors through controlled movements." },
                new RecoveryExercise { Id = 39, Name = "Pectoral Stretch", Description = "Relieves tightness in the chest muscles." },
                new RecoveryExercise { Id = 40, Name = "Bridge with Resistance Band", Description = "Enhances glute strength and hip stability." },
                new RecoveryExercise { Id = 41, Name = "Wrist Extensor Stretch", Description = "Stretches the extensors in the wrist for flexibility." },
                new RecoveryExercise { Id = 42, Name = "Side-Lying Clamshells", Description = "Strengthens the hip abductors and improves balance." },
                new RecoveryExercise { Id = 43, Name = "Wall Angels", Description = "Improves shoulder mobility and posture." },
                new RecoveryExercise { Id = 44, Name = "Child's Pose", Description = "Relieves lower back and hip tension." },
                new RecoveryExercise { Id = 45, Name = "Triceps Stretch", Description = "Stretches the triceps for better flexibility." },
                new RecoveryExercise { Id = 46, Name = "Scapular Push-ups", Description = "Strengthens the scapular stabilizers." },
                new RecoveryExercise { Id = 47, Name = "Ankle Inversion/Eversion", Description = "Enhances lateral ankle stability." },
                new RecoveryExercise { Id = 48, Name = "Hamstring Foam Rolling", Description = "Releases tension in the hamstrings." },
                new RecoveryExercise { Id = 49, Name = "Forearm Pronation/Supination", Description = "Improves wrist and forearm flexibility." },
                new RecoveryExercise { Id = 50, Name = "Bird Dog Exercise", Description = "Strengthens core and improves spinal stability." }
            );

            builder.Entity<Injury>().HasData(
                new Injury { Id = 1, Name = "Hamstring Strain", Description = "Hamstring Strain.", BodyPart = "Leg" },
                new Injury { Id = 2, Name = "Quadriceps Strain", Description = "Quadriceps Strain.", BodyPart = "Leg" },
                new Injury { Id = 3, Name = "Calf Strain", Description = "Calf Strain.", BodyPart = "Leg" },
                new Injury { Id = 4, Name = "Plantar Fasciitis", Description = "Inflammation of the plantar fascia.", BodyPart = "Foot" },
                new Injury { Id = 5, Name = "Sprained Ankle", Description = "Ligament injury in the ankle.", BodyPart = "Ankle" },
                new Injury { Id = 6, Name = "Shin Splints", Description = "Pain along the shin bone due to overuse.", BodyPart = "Leg" },
                new Injury { Id = 7, Name = "ACL Sprain", Description = "Anterior cruciate ligament injury.", BodyPart = "Knee" },
                new Injury { Id = 8, Name = "MCL Sprain", Description = "Medial collateral ligament sprain.", BodyPart = "Knee" },
                new Injury { Id = 9, Name = "Frozen Shoulder", Description = "Stiffness and pain in the shoulder joint.", BodyPart = "Shoulder" },
                new Injury { Id = 10, Name = "Rotator Cuff Tear", Description = "Injury to the rotator cuff muscles or tendons.", BodyPart = "Shoulder" },
                new Injury { Id = 11, Name = "Dislocated Shoulder", Description = "Shoulder joint displacement.", BodyPart = "Shoulder" },
                new Injury { Id = 12, Name = "Tennis Elbow", Description = "Overuse injury of the elbow.", BodyPart = "Elbow" },
                new Injury { Id = 13, Name = "Groin Strain", Description = "Strain in the groin muscles.", BodyPart = "Groin" },
                new Injury { Id = 14, Name = "Patellar Tendonitis", Description = "Inflammation of the patellar tendon.", BodyPart = "Knee" },
                new Injury { Id = 15, Name = "Achilles Tendonitis", Description = "Inflammation of the Achilles tendon.", BodyPart = "Leg" },
                new Injury { Id = 16, Name = "IT Band Syndrome", Description = "Tightness in the iliotibial band causing knee pain.", BodyPart = "Leg" },
                new Injury { Id = 17, Name = "Hip Flexor Strain", Description = "Injury to the hip flexor muscles.", BodyPart = "Hip" },
                new Injury { Id = 18, Name = "Lower Back Pain", Description = "Chronic or acute pain in the lower back.", BodyPart = "Back" },
                new Injury { Id = 19, Name = "Neck Strain", Description = "Injury to the neck muscles.", BodyPart = "Neck" },
                new Injury { Id = 20, Name = "Finger Tendonitis", Description = "Inflammation of the finger tendons.", BodyPart = "Hand" },
                new Injury { Id = 21, Name = "Scapular Dyskinesis", Description = "Improper movement of the shoulder blade.", BodyPart = "Shoulder" },
                new Injury { Id = 22, Name = "Bicep Tendonitis", Description = "Inflammation of the bicep tendon.", BodyPart = "Arm" },
                new Injury { Id = 23, Name = "Pectoral Strain", Description = "Strain in the chest muscles.", BodyPart = "Chest" },
                new Injury { Id = 24, Name = "Thoracic Outlet Syndrome", Description = "Compression in the upper chest area.", BodyPart = "Chest" },
                new Injury { Id = 25, Name = "Sciatica", Description = "Pain radiating along the sciatic nerve.", BodyPart = "Lower Back" },
                new Injury { Id = 26, Name = "Hip Labral Tear", Description = "Tear in the labrum of the hip joint.", BodyPart = "Hip" },
                new Injury { Id = 27, Name = "Cubital Tunnel Syndrome", Description = "Nerve compression in the elbow.", BodyPart = "Arm" },
                new Injury { Id = 28, Name = "Plantar Plate Injury", Description = "Damage to the ligament under the toe.", BodyPart = "Foot" },
                new Injury { Id = 29, Name = "Facet Joint Syndrome", Description = "Pain from the joints of the spine.", BodyPart = "Spine" },
                new Injury { Id = 30, Name = "Tight Hip Flexors", Description = "Limited flexibility in the hip flexor muscles.", BodyPart = "Hip" }
            );

            builder.Entity<InjuryRecoveryExercise>().HasData(
                // Hamstring Strain
                new InjuryRecoveryExercise { InjuryId = 1, RecoveryExerciseId = 1 }, // Hamstring Stretch
                new InjuryRecoveryExercise { InjuryId = 1, RecoveryExerciseId = 13 }, // Groin Stretch

                // Quadriceps Strain
                new InjuryRecoveryExercise { InjuryId = 2, RecoveryExerciseId = 2 }, // Quadriceps Stretch
                new InjuryRecoveryExercise { InjuryId = 2, RecoveryExerciseId = 14 }, // Patellar Tendon Exercises

                // Calf Strain
                new InjuryRecoveryExercise { InjuryId = 3, RecoveryExerciseId = 3 }, // Calf Raises
                new InjuryRecoveryExercise { InjuryId = 3, RecoveryExerciseId = 32 }, // Heel Drops

                // Plantar Fasciitis
                new InjuryRecoveryExercise { InjuryId = 4, RecoveryExerciseId = 4 }, // Plantar Fascia Stretch
                new InjuryRecoveryExercise { InjuryId = 4, RecoveryExerciseId = 26 }, // Toe Spreading

                // Sprained Ankle
                new InjuryRecoveryExercise { InjuryId = 5, RecoveryExerciseId = 5 }, // Ankle Proprioception Exercises
                new InjuryRecoveryExercise { InjuryId = 5, RecoveryExerciseId = 18 }, // Ankle Dorsiflexion Strengthening

                // Shin Splints
                new InjuryRecoveryExercise { InjuryId = 6, RecoveryExerciseId = 6 }, // Shin Splint Relief
                new InjuryRecoveryExercise { InjuryId = 6, RecoveryExerciseId = 21 }, // Glute Bridge

                // ACL Sprain
                new InjuryRecoveryExercise { InjuryId = 7, RecoveryExerciseId = 7 }, // ACL Strengthening
                new InjuryRecoveryExercise { InjuryId = 7, RecoveryExerciseId = 25 }, // Knee Wall Slides

                // MCL Sprain
                new InjuryRecoveryExercise { InjuryId = 8, RecoveryExerciseId = 8 }, // MCL Rehabilitation Stretch
                new InjuryRecoveryExercise { InjuryId = 8, RecoveryExerciseId = 14 }, // Patellar Tendon Exercises

                // Frozen Shoulder
                new InjuryRecoveryExercise { InjuryId = 9, RecoveryExerciseId = 11 }, // Frozen Shoulder Stretch
                new InjuryRecoveryExercise { InjuryId = 9, RecoveryExerciseId = 9 }, // Shoulder Pendulum Exercises

                // Rotator Cuff Tear
                new InjuryRecoveryExercise { InjuryId = 10, RecoveryExerciseId = 10 }, // Rotator Cuff Strengthening
                new InjuryRecoveryExercise { InjuryId = 10, RecoveryExerciseId = 33 }, // Scapular Retraction

                // Dislocated Shoulder
                new InjuryRecoveryExercise { InjuryId = 11, RecoveryExerciseId = 15 }, // Dislocated Shoulder Recovery
                new InjuryRecoveryExercise { InjuryId = 11, RecoveryExerciseId = 9 }, // Shoulder Pendulum Exercises

                // Tennis Elbow
                new InjuryRecoveryExercise { InjuryId = 12, RecoveryExerciseId = 27 }, // Tennis Elbow Isometric Holds
                new InjuryRecoveryExercise { InjuryId = 12, RecoveryExerciseId = 12 }, // Wrist Flexor Stretch

                // Groin Strain
                new InjuryRecoveryExercise { InjuryId = 13, RecoveryExerciseId = 13 }, // Groin Stretch
                new InjuryRecoveryExercise { InjuryId = 13, RecoveryExerciseId = 16 }, // Hip Flexor Stretch

                // Patellar Tendonitis
                new InjuryRecoveryExercise { InjuryId = 14, RecoveryExerciseId = 14 }, // Patellar Tendon Exercises
                new InjuryRecoveryExercise { InjuryId = 14, RecoveryExerciseId = 25 }, // Knee Wall Slides

                // Achilles Tendonitis
                new InjuryRecoveryExercise { InjuryId = 15, RecoveryExerciseId = 19 }, // Achilles Tendon Stretch
                new InjuryRecoveryExercise { InjuryId = 15, RecoveryExerciseId = 32 }, // Heel Drops

                // IT Band Syndrome
                new InjuryRecoveryExercise { InjuryId = 16, RecoveryExerciseId = 23 }, // IT Band Foam Rolling
                new InjuryRecoveryExercise { InjuryId = 16, RecoveryExerciseId = 22 }, // Hip Abduction Exercise

                // Hip Flexor Strain
                new InjuryRecoveryExercise { InjuryId = 17, RecoveryExerciseId = 16 }, // Hip Flexor Stretch
                new InjuryRecoveryExercise { InjuryId = 17, RecoveryExerciseId = 22 }, // Hip Abduction Exercise

                // Lower Back Pain
                new InjuryRecoveryExercise { InjuryId = 18, RecoveryExerciseId = 20 }, // Lower Back Extension
                new InjuryRecoveryExercise { InjuryId = 18, RecoveryExerciseId = 21 }, // Glute Bridge

                // Neck Strain
                new InjuryRecoveryExercise { InjuryId = 19, RecoveryExerciseId = 17 }, // Neck Mobility Exercises
                new InjuryRecoveryExercise { InjuryId = 19, RecoveryExerciseId = 24 }, // Cervical Retraction

                // Finger Tendonitis
                new InjuryRecoveryExercise { InjuryId = 20, RecoveryExerciseId = 30 }, // Finger Flexion Strengthening
                new InjuryRecoveryExercise { InjuryId = 20, RecoveryExerciseId = 35 },  // Dynamic Wrist Rotations
                // Scapular Dyskinesis
                new InjuryRecoveryExercise { InjuryId = 21, RecoveryExerciseId = 46 }, // Scapular Push-ups
                new InjuryRecoveryExercise { InjuryId = 21, RecoveryExerciseId = 43 }, // Wall Angels

                // Bicep Tendonitis
                new InjuryRecoveryExercise { InjuryId = 22, RecoveryExerciseId = 45 }, // Triceps Stretch
                new InjuryRecoveryExercise { InjuryId = 22, RecoveryExerciseId = 39 }, // Pectoral Stretch

                // Pectoral Strain
                new InjuryRecoveryExercise { InjuryId = 23, RecoveryExerciseId = 39 }, // Pectoral Stretch
                new InjuryRecoveryExercise { InjuryId = 23, RecoveryExerciseId = 36 }, // Shoulder Blade Squeeze

                // Thoracic Outlet Syndrome
                new InjuryRecoveryExercise { InjuryId = 24, RecoveryExerciseId = 43 }, // Wall Angels
                new InjuryRecoveryExercise { InjuryId = 24, RecoveryExerciseId = 36 }, // Shoulder Blade Squeeze

                // Sciatica
                new InjuryRecoveryExercise { InjuryId = 25, RecoveryExerciseId = 44 }, // Child's Pose
                new InjuryRecoveryExercise { InjuryId = 25, RecoveryExerciseId = 50 }, // Bird Dog Exercise

                // Hip Labral Tear
                new InjuryRecoveryExercise { InjuryId = 26, RecoveryExerciseId = 40 }, // Bridge with Resistance Band
                new InjuryRecoveryExercise { InjuryId = 26, RecoveryExerciseId = 42 }, // Side-Lying Clamshells

                // Cubital Tunnel Syndrome
                new InjuryRecoveryExercise { InjuryId = 27, RecoveryExerciseId = 49 }, // Forearm Pronation/Supination
                new InjuryRecoveryExercise { InjuryId = 27, RecoveryExerciseId = 41 }, // Wrist Extensor Stretch

                // Plantar Plate Injury
                new InjuryRecoveryExercise { InjuryId = 28, RecoveryExerciseId = 26 }, // Toe Spreading
                new InjuryRecoveryExercise { InjuryId = 28, RecoveryExerciseId = 4 },  // Plantar Fascia Stretch

                // Facet Joint Syndrome
                new InjuryRecoveryExercise { InjuryId = 29, RecoveryExerciseId = 37 }, // Cat-Cow Stretch
                new InjuryRecoveryExercise { InjuryId = 29, RecoveryExerciseId = 50 }, // Bird Dog Exercise

                // Tight Hip Flexors
                new InjuryRecoveryExercise { InjuryId = 30, RecoveryExerciseId = 16 }, // Hip Flexor Stretch
                new InjuryRecoveryExercise { InjuryId = 30, RecoveryExerciseId = 38 }  // Hip Flexor Activation
            );
        }
    }
}