#nullable enable

using System;
using System.Collections.Generic;
using AttendanceSystem.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Infrastructure.Persistence;

public partial class AttendanceDbContext : DbContext
{
    public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AttendanceCode> AttendanceCodes { get; set; }

    public virtual DbSet<AttendanceSubmissionLog> AttendanceSubmissionLogs { get; set; }

    public virtual DbSet<School> Schools { get; set; }

    public virtual DbSet<SchoolTerm> SchoolTerms { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentAlert> StudentAlerts { get; set; }

    public virtual DbSet<StudentAttendance> StudentAttendances { get; set; }

    public virtual DbSet<StudentAttendanceSummary> StudentAttendanceSummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttendanceCode>(entity =>
        {
            entity.HasKey(e => e.CodeId);

            entity.HasIndex(e => e.CodeValue, "UQ_AttendanceCodes_CodeValue").IsUnique();

            entity.Property(e => e.CodeId).HasColumnName("CodeID");
            entity.Property(e => e.CodeValue)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<AttendanceSubmissionLog>(entity =>
        {
            entity.HasKey(e => e.SubmissionId);

            entity.ToTable("AttendanceSubmissionLog");

            entity.Property(e => e.SubmissionId).HasColumnName("SubmissionID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedDate).HasColumnType("datetime");

            entity.HasOne(d => d.School).WithMany(p => p.AttendanceSubmissionLogs)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttendanceSubmissionLog_Schools");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.Active).HasDefaultValue(true, "DF_Schools_Active");
            entity.Property(e => e.SchoolName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TimeZone)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasDefaultValue("UTC", "DF_Schools_TimeZone");
        });

        modelBuilder.Entity<SchoolTerm>(entity =>
        {
            entity.HasKey(e => e.TermId);

            entity.HasIndex(e => new { e.SchoolId, e.SchoolYear }, "IX_SchoolTerms_SchoolID_SchoolYear");

            entity.Property(e => e.TermId).HasColumnName("TermID");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SchoolYear)
                .HasMaxLength(9)
                .IsUnicode(false);
            entity.Property(e => e.TermName)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.School).WithMany(p => p.SchoolTerms)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SchoolTerms_Schools");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Grade)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");

            entity.HasOne(d => d.School).WithMany(p => p.Students)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Students_Schools");
        });

        modelBuilder.Entity<StudentAlert>(entity =>
        {
            entity.HasKey(e => e.AlertId);

            entity.HasIndex(e => new { e.StudentId, e.SchoolId, e.SchoolYear, e.AlertType }, "UX_StudentAlerts_Open")
                .IsUnique()
                .HasFilter("([ResolvedDate] IS NULL)");

            entity.Property(e => e.AlertId).HasColumnName("AlertID");
            entity.Property(e => e.AlertDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.AlertMessage)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.AlertType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ResolvedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ResolvedDate).HasColumnType("datetime");
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SchoolYear)
                .HasMaxLength(9)
                .IsUnicode(false);
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.School).WithMany(p => p.StudentAlerts)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentAlerts_Schools");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentAlerts)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentAlerts_Students");
        });

        modelBuilder.Entity<StudentAttendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId);

            entity.ToTable("StudentAttendance");

            entity.HasIndex(e => new { e.StudentId, e.AttendDate }, "UX_StudentAttendance_StudentID_AttendDate").IsUnique();

            entity.Property(e => e.AttendanceId).HasColumnName("AttendanceID");
            entity.Property(e => e.AttendCode)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MinutesLate).HasDefaultValue(0);
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.TermId).HasColumnName("TermID");

            entity.HasOne(d => d.AttendCodeNavigation).WithMany(p => p.StudentAttendances)
                .HasPrincipalKey(p => p.CodeValue)
                .HasForeignKey(d => d.AttendCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentAttendance_AttendanceCodes");

            entity.HasOne(d => d.School).WithMany(p => p.StudentAttendances)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentAttendance_Schools");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentAttendances)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentAttendance_Students");

            entity.HasOne(d => d.Term).WithMany(p => p.StudentAttendances)
                .HasForeignKey(d => d.TermId)
                .HasConstraintName("FK_StudentAttendance_SchoolTerms");
        });

        modelBuilder.Entity<StudentAttendanceSummary>(entity =>
        {
            entity.HasKey(e => e.SummaryId);

            entity.ToTable("StudentAttendanceSummary");

            entity.HasIndex(e => new { e.StudentId, e.SchoolId, e.SchoolYear }, "UQ_StudentAttendanceSummary_StudentID_SchoolID_SchoolYear").IsUnique();

            entity.Property(e => e.SummaryId).HasColumnName("SummaryID");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SchoolId).HasColumnName("SchoolID");
            entity.Property(e => e.SchoolYear)
                .HasMaxLength(9)
                .IsUnicode(false);
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.School).WithMany(p => p.StudentAttendanceSummaries)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentAttendanceSummary_Schools");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentAttendanceSummaries)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentAttendanceSummary_Students");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
