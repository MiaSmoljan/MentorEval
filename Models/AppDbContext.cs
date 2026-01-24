using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MentorEval.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Evaluation> Evaluations { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Answers__3214EC077C308087");

            entity.HasIndex(e => new { e.EvaluationId, e.QuestionId }, "IX_Answers_Eval_Question");

            entity.HasOne(d => d.Evaluation).WithMany(p => p.Answers)
                .HasForeignKey(d => d.EvaluationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Answer_Evaluation");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_Answer_Question");

            entity.HasOne(d => d.Student).WithMany(p => p.Answers)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK_Answer_Student");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Courses__3214EC0792938969");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Professor).WithMany(p => p.Courses)
                .HasForeignKey(d => d.ProfessorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Course_Professor");
        });

        modelBuilder.Entity<Evaluation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Evaluati__3214EC076A51EA0C");

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Course).WithMany(p => p.Evaluations)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Evaluation_Course");

            entity.HasMany(d => d.Students).WithMany(p => p.Evaluations)
                .UsingEntity<Dictionary<string, object>>(
                    "EvaluationStudent",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("StudentId")
                        .HasConstraintName("FK_EvalStudent_Student"),
                    l => l.HasOne<Evaluation>().WithMany()
                        .HasForeignKey("EvaluationId")
                        .HasConstraintName("FK_EvalStudent_Evaluation"),
                    j =>
                    {
                        j.HasKey("EvaluationId", "StudentId");
                        j.ToTable("EvaluationStudent");
                    });
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3214EC07EFAF25BB");

            entity.Property(e => e.Required).HasDefaultValue(true);
            entity.Property(e => e.Text).HasMaxLength(500);
            entity.Property(e => e.Type).HasMaxLength(20);

            entity.HasOne(d => d.Evaluation).WithMany(p => p.Questions)
                .HasForeignKey(d => d.EvaluationId)
                .HasConstraintName("FK_Question_Evaluation");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reports__3214EC0772C7C3A7");

            entity.HasIndex(e => e.EvaluationId, "UQ_Report_Evaluation").IsUnique();

            entity.Property(e => e.Type).HasMaxLength(20);

            entity.HasOne(d => d.Evaluation).WithOne(p => p.Report)
                .HasForeignKey<Report>(d => d.EvaluationId)
                .HasConstraintName("FK_Report_Evaluation");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07F5069EC8");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4A70E823B").IsUnique();

            entity.Property(e => e.Discriminator).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(200);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

    }
}
