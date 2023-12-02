; ModuleID = 'builtins.c'
source_filename = "builtins.c"
target datalayout = "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-unknown-linux-gnu"

%struct.Array = type { i64, i64, i64, i8* }

; Function Attrs: nounwind sspstrong uwtable
define void @incrementLength(%struct.Array* nocapture %0, i64 %1) local_unnamed_addr #0 {
  %3 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 2
  %4 = load i64, i64* %3, align 8, !tbaa !3
  %5 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 1
  %6 = load i64, i64* %5, align 8, !tbaa !9
  %7 = add i64 %4, 1
  store i64 %7, i64* %3, align 8, !tbaa !3
  %8 = icmp eq i64 %6, %4
  br i1 %8, label %9, label %16

9:                                                ; preds = %2
  %10 = mul i64 %4, 3
  %11 = lshr i64 %10, 1
  store i64 %11, i64* %5, align 8, !tbaa !9
  %12 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 3
  %13 = load i8*, i8** %12, align 8, !tbaa !10
  %14 = mul i64 %11, %1
  %15 = tail call i8* @realloc(i8* %13, i64 %14) #6
  store i8* %15, i8** %12, align 8, !tbaa !10
  br label %16

16:                                               ; preds = %9, %2
  ret void
}

; Function Attrs: nounwind
declare noalias i8* @realloc(i8* nocapture, i64) local_unnamed_addr #1

; Function Attrs: nounwind sspstrong uwtable
define void @requireCapacity(%struct.Array* nocapture %0, i64 %1, i64 %2) local_unnamed_addr #0 {
  %4 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 1
  %5 = load i64, i64* %4, align 8, !tbaa !9
  %6 = icmp ult i64 %5, %1
  br i1 %6, label %7, label %12

7:                                                ; preds = %3
  store i64 %1, i64* %4, align 8, !tbaa !9
  %8 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 3
  %9 = load i8*, i8** %8, align 8, !tbaa !10
  %10 = mul i64 %2, %1
  %11 = tail call i8* @realloc(i8* %9, i64 %10) #6
  store i8* %11, i8** %8, align 8, !tbaa !10
  br label %12

12:                                               ; preds = %7, %3
  ret void
}

; Function Attrs: nounwind sspstrong uwtable
define void @shrinkMem(%struct.Array* nocapture %0, i64 %1) local_unnamed_addr #0 {
  %3 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 2
  %4 = load i64, i64* %3, align 8, !tbaa !3
  %5 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 3
  %6 = load i8*, i8** %5, align 8, !tbaa !10
  %7 = mul i64 %4, %1
  %8 = tail call i8* @realloc(i8* %6, i64 %7) #6
  store i8* %8, i8** %5, align 8, !tbaa !10
  %9 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 1
  store i64 %4, i64* %9, align 8, !tbaa !9
  ret void
}

; Function Attrs: nofree nounwind sspstrong uwtable
define void @removeAt(%struct.Array* nocapture %0, i64 %1, i64 %2) local_unnamed_addr #2 {
  %4 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 2
  %5 = load i64, i64* %4, align 8, !tbaa !3
  %6 = add i64 %5, -1
  store i64 %6, i64* %4, align 8, !tbaa !3
  %7 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 3
  %8 = load i8*, i8** %7, align 8, !tbaa !10
  %9 = mul i64 %2, %1
  %10 = getelementptr i8, i8* %8, i64 %9
  %11 = getelementptr i8, i8* %10, i64 %2
  %12 = xor i64 %1, -1
  %13 = add i64 %5, %12
  %14 = mul i64 %13, %2
  call void @llvm.memmove.p0i8.p0i8.i64(i8* nonnull align 1 %10, i8* nonnull align 1 %11, i64 %14, i1 false) #6
  ret void
}

; Function Attrs: nounwind sspstrong uwtable
define void @insertSpace(%struct.Array* nocapture %0, i64 %1, i64 %2) local_unnamed_addr #0 {
  %4 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 2
  %5 = load i64, i64* %4, align 8, !tbaa !3
  %6 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 1
  %7 = load i64, i64* %6, align 8, !tbaa !9
  %8 = add i64 %5, 1
  store i64 %8, i64* %4, align 8, !tbaa !3
  %9 = icmp eq i64 %7, %5
  br i1 %9, label %13, label %10

10:                                               ; preds = %3
  %11 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 3
  %12 = load i8*, i8** %11, align 8, !tbaa !10
  br label %20

13:                                               ; preds = %3
  %14 = mul i64 %5, 3
  %15 = lshr i64 %14, 1
  store i64 %15, i64* %6, align 8, !tbaa !9
  %16 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 3
  %17 = load i8*, i8** %16, align 8, !tbaa !10
  %18 = mul i64 %15, %2
  %19 = tail call i8* @realloc(i8* %17, i64 %18) #6
  store i8* %19, i8** %16, align 8, !tbaa !10
  br label %20

20:                                               ; preds = %10, %13
  %21 = phi i8* [ %12, %10 ], [ %19, %13 ]
  %22 = mul i64 %2, %1
  %23 = getelementptr i8, i8* %21, i64 %22
  %24 = getelementptr i8, i8* %23, i64 %2
  %25 = sub i64 %5, %1
  %26 = mul i64 %25, %2
  call void @llvm.memmove.p0i8.p0i8.i64(i8* nonnull align 1 %24, i8* nonnull align 1 %23, i64 %26, i1 false) #6
  ret void
}

; Function Attrs: nofree norecurse nounwind sspstrong uwtable
define void @incrementArrayRefCounts(%struct.Array* nocapture readonly %0, i64 %1) local_unnamed_addr #3 {
  %3 = and i64 %1, 1
  %4 = icmp eq i64 %3, 0
  br i1 %4, label %5, label %60

5:                                                ; preds = %2
  %6 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 2
  %7 = load i64, i64* %6, align 8, !tbaa !3
  %8 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 3
  %9 = load i8*, i8** %8, align 8, !tbaa !10
  %10 = icmp eq i64 %7, 0
  br i1 %10, label %60, label %11

11:                                               ; preds = %5
  %12 = add i64 %7, -1
  %13 = and i64 %7, 3
  %14 = icmp ult i64 %12, 3
  br i1 %14, label %46, label %15

15:                                               ; preds = %11
  %16 = and i64 %7, -4
  br label %17

17:                                               ; preds = %17, %15
  %18 = phi i64 [ 0, %15 ], [ %43, %17 ]
  %19 = phi i64 [ %16, %15 ], [ %44, %17 ]
  %20 = mul i64 %18, %1
  %21 = getelementptr i8, i8* %9, i64 %20
  %22 = bitcast i8* %21 to i64*
  %23 = load i64, i64* %22, align 8, !tbaa !11
  %24 = add i64 %23, 1
  store i64 %24, i64* %22, align 8, !tbaa !11
  %25 = or i64 %18, 1
  %26 = mul i64 %25, %1
  %27 = getelementptr i8, i8* %9, i64 %26
  %28 = bitcast i8* %27 to i64*
  %29 = load i64, i64* %28, align 8, !tbaa !11
  %30 = add i64 %29, 1
  store i64 %30, i64* %28, align 8, !tbaa !11
  %31 = or i64 %18, 2
  %32 = mul i64 %31, %1
  %33 = getelementptr i8, i8* %9, i64 %32
  %34 = bitcast i8* %33 to i64*
  %35 = load i64, i64* %34, align 8, !tbaa !11
  %36 = add i64 %35, 1
  store i64 %36, i64* %34, align 8, !tbaa !11
  %37 = or i64 %18, 3
  %38 = mul i64 %37, %1
  %39 = getelementptr i8, i8* %9, i64 %38
  %40 = bitcast i8* %39 to i64*
  %41 = load i64, i64* %40, align 8, !tbaa !11
  %42 = add i64 %41, 1
  store i64 %42, i64* %40, align 8, !tbaa !11
  %43 = add nuw i64 %18, 4
  %44 = add i64 %19, -4
  %45 = icmp eq i64 %44, 0
  br i1 %45, label %46, label %17

46:                                               ; preds = %17, %11
  %47 = phi i64 [ 0, %11 ], [ %43, %17 ]
  %48 = icmp eq i64 %13, 0
  br i1 %48, label %60, label %49

49:                                               ; preds = %46, %49
  %50 = phi i64 [ %57, %49 ], [ %47, %46 ]
  %51 = phi i64 [ %58, %49 ], [ %13, %46 ]
  %52 = mul i64 %50, %1
  %53 = getelementptr i8, i8* %9, i64 %52
  %54 = bitcast i8* %53 to i64*
  %55 = load i64, i64* %54, align 8, !tbaa !11
  %56 = add i64 %55, 1
  store i64 %56, i64* %54, align 8, !tbaa !11
  %57 = add nuw i64 %50, 1
  %58 = add i64 %51, -1
  %59 = icmp eq i64 %58, 0
  br i1 %59, label %60, label %49, !llvm.loop !12

60:                                               ; preds = %46, %49, %5, %2
  ret void
}

; Function Attrs: nofree norecurse nounwind sspstrong uwtable
define void @alwaysIncrementArrayRefCounts(%struct.Array* nocapture readonly %0, i64 %1) local_unnamed_addr #3 {
  %3 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 2
  %4 = load i64, i64* %3, align 8, !tbaa !3
  %5 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 3
  %6 = load i8*, i8** %5, align 8, !tbaa !10
  %7 = icmp eq i64 %4, 0
  br i1 %7, label %28, label %8

8:                                                ; preds = %2
  %9 = add i64 %4, -1
  %10 = and i64 %4, 3
  %11 = icmp ult i64 %9, 3
  br i1 %11, label %14, label %12

12:                                               ; preds = %8
  %13 = and i64 %4, -4
  br label %29

14:                                               ; preds = %29, %8
  %15 = phi i64 [ 0, %8 ], [ %55, %29 ]
  %16 = icmp eq i64 %10, 0
  br i1 %16, label %28, label %17

17:                                               ; preds = %14, %17
  %18 = phi i64 [ %25, %17 ], [ %15, %14 ]
  %19 = phi i64 [ %26, %17 ], [ %10, %14 ]
  %20 = mul i64 %18, %1
  %21 = getelementptr i8, i8* %6, i64 %20
  %22 = bitcast i8* %21 to i64*
  %23 = load i64, i64* %22, align 8, !tbaa !11
  %24 = add i64 %23, 1
  store i64 %24, i64* %22, align 8, !tbaa !11
  %25 = add nuw i64 %18, 1
  %26 = add i64 %19, -1
  %27 = icmp eq i64 %26, 0
  br i1 %27, label %28, label %17, !llvm.loop !14

28:                                               ; preds = %14, %17, %2
  ret void

29:                                               ; preds = %29, %12
  %30 = phi i64 [ 0, %12 ], [ %55, %29 ]
  %31 = phi i64 [ %13, %12 ], [ %56, %29 ]
  %32 = mul i64 %30, %1
  %33 = getelementptr i8, i8* %6, i64 %32
  %34 = bitcast i8* %33 to i64*
  %35 = load i64, i64* %34, align 8, !tbaa !11
  %36 = add i64 %35, 1
  store i64 %36, i64* %34, align 8, !tbaa !11
  %37 = or i64 %30, 1
  %38 = mul i64 %37, %1
  %39 = getelementptr i8, i8* %6, i64 %38
  %40 = bitcast i8* %39 to i64*
  %41 = load i64, i64* %40, align 8, !tbaa !11
  %42 = add i64 %41, 1
  store i64 %42, i64* %40, align 8, !tbaa !11
  %43 = or i64 %30, 2
  %44 = mul i64 %43, %1
  %45 = getelementptr i8, i8* %6, i64 %44
  %46 = bitcast i8* %45 to i64*
  %47 = load i64, i64* %46, align 8, !tbaa !11
  %48 = add i64 %47, 1
  store i64 %48, i64* %46, align 8, !tbaa !11
  %49 = or i64 %30, 3
  %50 = mul i64 %49, %1
  %51 = getelementptr i8, i8* %6, i64 %50
  %52 = bitcast i8* %51 to i64*
  %53 = load i64, i64* %52, align 8, !tbaa !11
  %54 = add i64 %53, 1
  store i64 %54, i64* %52, align 8, !tbaa !11
  %55 = add nuw i64 %30, 4
  %56 = add i64 %31, -4
  %57 = icmp eq i64 %56, 0
  br i1 %57, label %14, label %29
}

; Function Attrs: nofree nounwind sspstrong uwtable
define noalias %struct.Array* @clone(%struct.Array* nocapture readonly %0, i64 %1) local_unnamed_addr #2 {
  %3 = tail call noalias dereferenceable_or_null(32) i8* @malloc(i64 32) #6
  %4 = bitcast i8* %3 to i64*
  store i64 0, i64* %4, align 8, !tbaa !15
  %5 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 1
  %6 = getelementptr inbounds i8, i8* %3, i64 8
  %7 = bitcast i64* %5 to <2 x i64>*
  %8 = load <2 x i64>, <2 x i64>* %7, align 8, !tbaa !11
  %9 = bitcast i8* %6 to <2 x i64>*
  store <2 x i64> %8, <2 x i64>* %9, align 8, !tbaa !11
  %10 = lshr i64 %1, 1
  %11 = extractelement <2 x i64> %8, i32 0
  %12 = mul i64 %11, %10
  %13 = tail call noalias i8* @malloc(i64 %12) #6
  %14 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 3
  %15 = load i8*, i8** %14, align 8, !tbaa !10
  call void @llvm.memcpy.p0i8.p0i8.i64(i8* nonnull align 1 %13, i8* nonnull align 1 %15, i64 %12, i1 false) #6
  %16 = getelementptr inbounds i8, i8* %3, i64 24
  %17 = bitcast i8* %16 to i8**
  store i8* %13, i8** %17, align 8, !tbaa !10
  %18 = and i64 %1, 1
  %19 = icmp eq i64 %18, 0
  br i1 %19, label %20, label %74

20:                                               ; preds = %2
  %21 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 2
  %22 = load i64, i64* %21, align 8, !tbaa !3
  %23 = load i8*, i8** %14, align 8, !tbaa !10
  %24 = icmp eq i64 %22, 0
  br i1 %24, label %74, label %25

25:                                               ; preds = %20
  %26 = add i64 %22, -1
  %27 = and i64 %22, 3
  %28 = icmp ult i64 %26, 3
  br i1 %28, label %60, label %29

29:                                               ; preds = %25
  %30 = and i64 %22, -4
  br label %31

31:                                               ; preds = %31, %29
  %32 = phi i64 [ 0, %29 ], [ %57, %31 ]
  %33 = phi i64 [ %30, %29 ], [ %58, %31 ]
  %34 = mul i64 %32, %1
  %35 = getelementptr i8, i8* %23, i64 %34
  %36 = bitcast i8* %35 to i64*
  %37 = load i64, i64* %36, align 8, !tbaa !11
  %38 = add i64 %37, 1
  store i64 %38, i64* %36, align 8, !tbaa !11
  %39 = or i64 %32, 1
  %40 = mul i64 %39, %1
  %41 = getelementptr i8, i8* %23, i64 %40
  %42 = bitcast i8* %41 to i64*
  %43 = load i64, i64* %42, align 8, !tbaa !11
  %44 = add i64 %43, 1
  store i64 %44, i64* %42, align 8, !tbaa !11
  %45 = or i64 %32, 2
  %46 = mul i64 %45, %1
  %47 = getelementptr i8, i8* %23, i64 %46
  %48 = bitcast i8* %47 to i64*
  %49 = load i64, i64* %48, align 8, !tbaa !11
  %50 = add i64 %49, 1
  store i64 %50, i64* %48, align 8, !tbaa !11
  %51 = or i64 %32, 3
  %52 = mul i64 %51, %1
  %53 = getelementptr i8, i8* %23, i64 %52
  %54 = bitcast i8* %53 to i64*
  %55 = load i64, i64* %54, align 8, !tbaa !11
  %56 = add i64 %55, 1
  store i64 %56, i64* %54, align 8, !tbaa !11
  %57 = add nuw i64 %32, 4
  %58 = add i64 %33, -4
  %59 = icmp eq i64 %58, 0
  br i1 %59, label %60, label %31

60:                                               ; preds = %31, %25
  %61 = phi i64 [ 0, %25 ], [ %57, %31 ]
  %62 = icmp eq i64 %27, 0
  br i1 %62, label %74, label %63

63:                                               ; preds = %60, %63
  %64 = phi i64 [ %71, %63 ], [ %61, %60 ]
  %65 = phi i64 [ %72, %63 ], [ %27, %60 ]
  %66 = mul i64 %64, %1
  %67 = getelementptr i8, i8* %23, i64 %66
  %68 = bitcast i8* %67 to i64*
  %69 = load i64, i64* %68, align 8, !tbaa !11
  %70 = add i64 %69, 1
  store i64 %70, i64* %68, align 8, !tbaa !11
  %71 = add nuw i64 %64, 1
  %72 = add i64 %65, -1
  %73 = icmp eq i64 %72, 0
  br i1 %73, label %74, label %63, !llvm.loop !16

74:                                               ; preds = %60, %63, %2, %20
  %75 = bitcast i8* %3 to %struct.Array*
  ret %struct.Array* %75
}

; Function Attrs: nofree nounwind
declare noalias i8* @malloc(i64) local_unnamed_addr #4

; Function Attrs: nounwind sspstrong uwtable
define void @extend(%struct.Array* nocapture %0, %struct.Array* nocapture readonly %1, i64 %2) local_unnamed_addr #0 {
  %4 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 2
  %5 = load i64, i64* %4, align 8, !tbaa !3
  %6 = getelementptr inbounds %struct.Array, %struct.Array* %1, i64 0, i32 2
  %7 = load i64, i64* %6, align 8, !tbaa !3
  %8 = add i64 %7, %5
  %9 = lshr i64 %2, 1
  %10 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 1
  %11 = load i64, i64* %10, align 8, !tbaa !9
  %12 = icmp ult i64 %11, %8
  br i1 %12, label %13, label %18

13:                                               ; preds = %3
  store i64 %8, i64* %10, align 8, !tbaa !9
  %14 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 3
  %15 = load i8*, i8** %14, align 8, !tbaa !10
  %16 = mul i64 %8, %9
  %17 = tail call i8* @realloc(i8* %15, i64 %16) #6
  store i8* %17, i8** %14, align 8, !tbaa !10
  br label %18

18:                                               ; preds = %3, %13
  store i64 %8, i64* %4, align 8, !tbaa !3
  %19 = and i64 %2, 1
  %20 = icmp eq i64 %19, 0
  br i1 %20, label %24, label %21

21:                                               ; preds = %18
  %22 = getelementptr inbounds %struct.Array, %struct.Array* %1, i64 0, i32 3
  %23 = load i8*, i8** %22, align 8, !tbaa !10
  br label %78

24:                                               ; preds = %18
  %25 = load i64, i64* %6, align 8, !tbaa !3
  %26 = getelementptr inbounds %struct.Array, %struct.Array* %1, i64 0, i32 3
  %27 = load i8*, i8** %26, align 8, !tbaa !10
  %28 = icmp eq i64 %25, 0
  br i1 %28, label %78, label %29

29:                                               ; preds = %24
  %30 = add i64 %25, -1
  %31 = and i64 %25, 3
  %32 = icmp ult i64 %30, 3
  br i1 %32, label %64, label %33

33:                                               ; preds = %29
  %34 = and i64 %25, -4
  br label %35

35:                                               ; preds = %35, %33
  %36 = phi i64 [ 0, %33 ], [ %61, %35 ]
  %37 = phi i64 [ %34, %33 ], [ %62, %35 ]
  %38 = mul i64 %36, %2
  %39 = getelementptr i8, i8* %27, i64 %38
  %40 = bitcast i8* %39 to i64*
  %41 = load i64, i64* %40, align 8, !tbaa !11
  %42 = add i64 %41, 1
  store i64 %42, i64* %40, align 8, !tbaa !11
  %43 = or i64 %36, 1
  %44 = mul i64 %43, %2
  %45 = getelementptr i8, i8* %27, i64 %44
  %46 = bitcast i8* %45 to i64*
  %47 = load i64, i64* %46, align 8, !tbaa !11
  %48 = add i64 %47, 1
  store i64 %48, i64* %46, align 8, !tbaa !11
  %49 = or i64 %36, 2
  %50 = mul i64 %49, %2
  %51 = getelementptr i8, i8* %27, i64 %50
  %52 = bitcast i8* %51 to i64*
  %53 = load i64, i64* %52, align 8, !tbaa !11
  %54 = add i64 %53, 1
  store i64 %54, i64* %52, align 8, !tbaa !11
  %55 = or i64 %36, 3
  %56 = mul i64 %55, %2
  %57 = getelementptr i8, i8* %27, i64 %56
  %58 = bitcast i8* %57 to i64*
  %59 = load i64, i64* %58, align 8, !tbaa !11
  %60 = add i64 %59, 1
  store i64 %60, i64* %58, align 8, !tbaa !11
  %61 = add nuw i64 %36, 4
  %62 = add i64 %37, -4
  %63 = icmp eq i64 %62, 0
  br i1 %63, label %64, label %35

64:                                               ; preds = %35, %29
  %65 = phi i64 [ 0, %29 ], [ %61, %35 ]
  %66 = icmp eq i64 %31, 0
  br i1 %66, label %78, label %67

67:                                               ; preds = %64, %67
  %68 = phi i64 [ %75, %67 ], [ %65, %64 ]
  %69 = phi i64 [ %76, %67 ], [ %31, %64 ]
  %70 = mul i64 %68, %2
  %71 = getelementptr i8, i8* %27, i64 %70
  %72 = bitcast i8* %71 to i64*
  %73 = load i64, i64* %72, align 8, !tbaa !11
  %74 = add i64 %73, 1
  store i64 %74, i64* %72, align 8, !tbaa !11
  %75 = add nuw i64 %68, 1
  %76 = add i64 %69, -1
  %77 = icmp eq i64 %76, 0
  br i1 %77, label %78, label %67, !llvm.loop !17

78:                                               ; preds = %64, %67, %21, %24
  %79 = phi i8* [ %23, %21 ], [ %27, %24 ], [ %27, %67 ], [ %27, %64 ]
  %80 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 3
  %81 = load i8*, i8** %80, align 8, !tbaa !10
  %82 = getelementptr i8, i8* %81, i64 %5
  %83 = mul i64 %7, %9
  call void @llvm.memcpy.p0i8.p0i8.i64(i8* nonnull align 1 %82, i8* nonnull align 1 %79, i64 %83, i1 false) #6
  ret void
}

; Function Attrs: nofree nounwind sspstrong uwtable
define noalias %struct.Array* @join(%struct.Array* nocapture readonly %0, %struct.Array* nocapture readonly %1, i64 %2) local_unnamed_addr #2 {
  %4 = tail call noalias dereferenceable_or_null(32) i8* @malloc(i64 32) #6
  %5 = bitcast i8* %4 to i64*
  store i64 0, i64* %5, align 8, !tbaa !15
  %6 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 2
  %7 = load i64, i64* %6, align 8, !tbaa !3
  %8 = getelementptr inbounds %struct.Array, %struct.Array* %1, i64 0, i32 2
  %9 = load i64, i64* %8, align 8, !tbaa !3
  %10 = add i64 %9, %7
  %11 = getelementptr inbounds i8, i8* %4, i64 16
  %12 = bitcast i8* %11 to i64*
  store i64 %10, i64* %12, align 8, !tbaa !3
  %13 = getelementptr inbounds i8, i8* %4, i64 8
  %14 = bitcast i8* %13 to i64*
  store i64 %10, i64* %14, align 8, !tbaa !9
  %15 = lshr i64 %2, 1
  %16 = mul i64 %10, %15
  %17 = tail call noalias i8* @malloc(i64 %16) #6
  %18 = getelementptr inbounds %struct.Array, %struct.Array* %0, i64 0, i32 3
  %19 = load i8*, i8** %18, align 8, !tbaa !10
  %20 = mul i64 %7, %15
  call void @llvm.memcpy.p0i8.p0i8.i64(i8* nonnull align 1 %17, i8* nonnull align 1 %19, i64 %20, i1 false) #6
  %21 = getelementptr i8, i8* %17, i64 %7
  %22 = getelementptr inbounds %struct.Array, %struct.Array* %1, i64 0, i32 3
  %23 = load i8*, i8** %22, align 8, !tbaa !10
  %24 = mul i64 %9, %15
  call void @llvm.memcpy.p0i8.p0i8.i64(i8* nonnull align 1 %21, i8* nonnull align 1 %23, i64 %24, i1 false) #6
  %25 = getelementptr inbounds i8, i8* %4, i64 24
  %26 = bitcast i8* %25 to i8**
  store i8* %17, i8** %26, align 8, !tbaa !10
  %27 = and i64 %2, 1
  %28 = icmp ne i64 %27, 0
  %29 = icmp eq i64 %10, 0
  %30 = or i1 %28, %29
  br i1 %30, label %81, label %31

31:                                               ; preds = %3
  %32 = add i64 %9, %7
  %33 = add i64 %32, -1
  %34 = and i64 %10, 3
  %35 = icmp ult i64 %33, 3
  br i1 %35, label %67, label %36

36:                                               ; preds = %31
  %37 = and i64 %10, -4
  br label %38

38:                                               ; preds = %38, %36
  %39 = phi i64 [ 0, %36 ], [ %64, %38 ]
  %40 = phi i64 [ %37, %36 ], [ %65, %38 ]
  %41 = mul i64 %39, %2
  %42 = getelementptr i8, i8* %17, i64 %41
  %43 = bitcast i8* %42 to i64*
  %44 = load i64, i64* %43, align 8, !tbaa !11
  %45 = add i64 %44, 1
  store i64 %45, i64* %43, align 8, !tbaa !11
  %46 = or i64 %39, 1
  %47 = mul i64 %46, %2
  %48 = getelementptr i8, i8* %17, i64 %47
  %49 = bitcast i8* %48 to i64*
  %50 = load i64, i64* %49, align 8, !tbaa !11
  %51 = add i64 %50, 1
  store i64 %51, i64* %49, align 8, !tbaa !11
  %52 = or i64 %39, 2
  %53 = mul i64 %52, %2
  %54 = getelementptr i8, i8* %17, i64 %53
  %55 = bitcast i8* %54 to i64*
  %56 = load i64, i64* %55, align 8, !tbaa !11
  %57 = add i64 %56, 1
  store i64 %57, i64* %55, align 8, !tbaa !11
  %58 = or i64 %39, 3
  %59 = mul i64 %58, %2
  %60 = getelementptr i8, i8* %17, i64 %59
  %61 = bitcast i8* %60 to i64*
  %62 = load i64, i64* %61, align 8, !tbaa !11
  %63 = add i64 %62, 1
  store i64 %63, i64* %61, align 8, !tbaa !11
  %64 = add nuw i64 %39, 4
  %65 = add i64 %40, -4
  %66 = icmp eq i64 %65, 0
  br i1 %66, label %67, label %38

67:                                               ; preds = %38, %31
  %68 = phi i64 [ 0, %31 ], [ %64, %38 ]
  %69 = icmp eq i64 %34, 0
  br i1 %69, label %81, label %70

70:                                               ; preds = %67, %70
  %71 = phi i64 [ %78, %70 ], [ %68, %67 ]
  %72 = phi i64 [ %79, %70 ], [ %34, %67 ]
  %73 = mul i64 %71, %2
  %74 = getelementptr i8, i8* %17, i64 %73
  %75 = bitcast i8* %74 to i64*
  %76 = load i64, i64* %75, align 8, !tbaa !11
  %77 = add i64 %76, 1
  store i64 %77, i64* %75, align 8, !tbaa !11
  %78 = add nuw i64 %71, 1
  %79 = add i64 %72, -1
  %80 = icmp eq i64 %79, 0
  br i1 %80, label %81, label %70, !llvm.loop !18

81:                                               ; preds = %67, %70, %3
  %82 = bitcast i8* %4 to %struct.Array*
  ret %struct.Array* %82
}

; Function Attrs: nofree nounwind sspstrong uwtable
define noalias %struct.Array* @rangeArray1(i32 %0) local_unnamed_addr #2 {
  %2 = tail call noalias dereferenceable_or_null(32) i8* @malloc(i64 32) #6
  %3 = bitcast i8* %2 to i64*
  store i64 0, i64* %3, align 8, !tbaa !15
  %4 = sext i32 %0 to i64
  %5 = getelementptr inbounds i8, i8* %2, i64 16
  %6 = bitcast i8* %5 to i64*
  store i64 %4, i64* %6, align 8, !tbaa !3
  %7 = getelementptr inbounds i8, i8* %2, i64 8
  %8 = bitcast i8* %7 to i64*
  store i64 %4, i64* %8, align 8, !tbaa !9
  %9 = shl nsw i64 %4, 2
  %10 = tail call noalias i8* @malloc(i64 %9) #6
  %11 = bitcast i8* %10 to i32*
  %12 = getelementptr inbounds i8, i8* %2, i64 24
  %13 = bitcast i8* %12 to i8**
  store i8* %10, i8** %13, align 8, !tbaa !10
  %14 = icmp sgt i32 %0, 0
  br i1 %14, label %15, label %82

15:                                               ; preds = %1
  %16 = zext i32 %0 to i64
  %17 = icmp ult i32 %0, 8
  br i1 %17, label %18, label %20

18:                                               ; preds = %80, %15
  %19 = phi i64 [ 0, %15 ], [ %21, %80 ]
  br label %84

20:                                               ; preds = %15
  %21 = and i64 %16, 4294967288
  %22 = add nsw i64 %21, -8
  %23 = lshr exact i64 %22, 3
  %24 = add nuw nsw i64 %23, 1
  %25 = and i64 %24, 3
  %26 = icmp ult i64 %22, 24
  br i1 %26, label %63, label %27

27:                                               ; preds = %20
  %28 = and i64 %24, 4611686018427387900
  br label %29

29:                                               ; preds = %29, %27
  %30 = phi i64 [ 0, %27 ], [ %59, %29 ]
  %31 = phi <4 x i32> [ <i32 0, i32 1, i32 2, i32 3>, %27 ], [ %60, %29 ]
  %32 = phi i64 [ %28, %27 ], [ %61, %29 ]
  %33 = getelementptr i32, i32* %11, i64 %30
  %34 = add <4 x i32> %31, <i32 4, i32 4, i32 4, i32 4>
  %35 = bitcast i32* %33 to <4 x i32>*
  store <4 x i32> %31, <4 x i32>* %35, align 4, !tbaa !19
  %36 = getelementptr i32, i32* %33, i64 4
  %37 = bitcast i32* %36 to <4 x i32>*
  store <4 x i32> %34, <4 x i32>* %37, align 4, !tbaa !19
  %38 = or i64 %30, 8
  %39 = add <4 x i32> %31, <i32 8, i32 8, i32 8, i32 8>
  %40 = getelementptr i32, i32* %11, i64 %38
  %41 = add <4 x i32> %31, <i32 12, i32 12, i32 12, i32 12>
  %42 = bitcast i32* %40 to <4 x i32>*
  store <4 x i32> %39, <4 x i32>* %42, align 4, !tbaa !19
  %43 = getelementptr i32, i32* %40, i64 4
  %44 = bitcast i32* %43 to <4 x i32>*
  store <4 x i32> %41, <4 x i32>* %44, align 4, !tbaa !19
  %45 = or i64 %30, 16
  %46 = add <4 x i32> %31, <i32 16, i32 16, i32 16, i32 16>
  %47 = getelementptr i32, i32* %11, i64 %45
  %48 = add <4 x i32> %31, <i32 20, i32 20, i32 20, i32 20>
  %49 = bitcast i32* %47 to <4 x i32>*
  store <4 x i32> %46, <4 x i32>* %49, align 4, !tbaa !19
  %50 = getelementptr i32, i32* %47, i64 4
  %51 = bitcast i32* %50 to <4 x i32>*
  store <4 x i32> %48, <4 x i32>* %51, align 4, !tbaa !19
  %52 = or i64 %30, 24
  %53 = add <4 x i32> %31, <i32 24, i32 24, i32 24, i32 24>
  %54 = getelementptr i32, i32* %11, i64 %52
  %55 = add <4 x i32> %31, <i32 28, i32 28, i32 28, i32 28>
  %56 = bitcast i32* %54 to <4 x i32>*
  store <4 x i32> %53, <4 x i32>* %56, align 4, !tbaa !19
  %57 = getelementptr i32, i32* %54, i64 4
  %58 = bitcast i32* %57 to <4 x i32>*
  store <4 x i32> %55, <4 x i32>* %58, align 4, !tbaa !19
  %59 = add i64 %30, 32
  %60 = add <4 x i32> %31, <i32 32, i32 32, i32 32, i32 32>
  %61 = add i64 %32, -4
  %62 = icmp eq i64 %61, 0
  br i1 %62, label %63, label %29, !llvm.loop !21

63:                                               ; preds = %29, %20
  %64 = phi i64 [ 0, %20 ], [ %59, %29 ]
  %65 = phi <4 x i32> [ <i32 0, i32 1, i32 2, i32 3>, %20 ], [ %60, %29 ]
  %66 = icmp eq i64 %25, 0
  br i1 %66, label %80, label %67

67:                                               ; preds = %63, %67
  %68 = phi i64 [ %76, %67 ], [ %64, %63 ]
  %69 = phi <4 x i32> [ %77, %67 ], [ %65, %63 ]
  %70 = phi i64 [ %78, %67 ], [ %25, %63 ]
  %71 = getelementptr i32, i32* %11, i64 %68
  %72 = add <4 x i32> %69, <i32 4, i32 4, i32 4, i32 4>
  %73 = bitcast i32* %71 to <4 x i32>*
  store <4 x i32> %69, <4 x i32>* %73, align 4, !tbaa !19
  %74 = getelementptr i32, i32* %71, i64 4
  %75 = bitcast i32* %74 to <4 x i32>*
  store <4 x i32> %72, <4 x i32>* %75, align 4, !tbaa !19
  %76 = add i64 %68, 8
  %77 = add <4 x i32> %69, <i32 8, i32 8, i32 8, i32 8>
  %78 = add i64 %70, -1
  %79 = icmp eq i64 %78, 0
  br i1 %79, label %80, label %67, !llvm.loop !23

80:                                               ; preds = %67, %63
  %81 = icmp eq i64 %21, %16
  br i1 %81, label %82, label %18

82:                                               ; preds = %84, %80, %1
  %83 = bitcast i8* %2 to %struct.Array*
  ret %struct.Array* %83

84:                                               ; preds = %18, %84
  %85 = phi i64 [ %88, %84 ], [ %19, %18 ]
  %86 = getelementptr i32, i32* %11, i64 %85
  %87 = trunc i64 %85 to i32
  store i32 %87, i32* %86, align 4, !tbaa !19
  %88 = add nuw nsw i64 %85, 1
  %89 = icmp eq i64 %88, %16
  br i1 %89, label %82, label %84, !llvm.loop !24
}

; Function Attrs: nofree nounwind sspstrong uwtable
define noalias %struct.Array* @rangeArray2(i32 %0, i32 %1) local_unnamed_addr #2 {
  %3 = sub i32 %1, %0
  %4 = tail call noalias dereferenceable_or_null(32) i8* @malloc(i64 32) #6
  %5 = bitcast i8* %4 to i64*
  store i64 0, i64* %5, align 8, !tbaa !15
  %6 = sext i32 %3 to i64
  %7 = getelementptr inbounds i8, i8* %4, i64 16
  %8 = bitcast i8* %7 to i64*
  store i64 %6, i64* %8, align 8, !tbaa !3
  %9 = getelementptr inbounds i8, i8* %4, i64 8
  %10 = bitcast i8* %9 to i64*
  store i64 %6, i64* %10, align 8, !tbaa !9
  %11 = shl nsw i64 %6, 2
  %12 = tail call noalias i8* @malloc(i64 %11) #6
  %13 = bitcast i8* %12 to i32*
  %14 = getelementptr inbounds i8, i8* %4, i64 24
  %15 = bitcast i8* %14 to i8**
  store i8* %12, i8** %15, align 8, !tbaa !10
  %16 = icmp sgt i32 %3, 0
  br i1 %16, label %17, label %77

17:                                               ; preds = %2
  %18 = zext i32 %3 to i64
  %19 = icmp ult i32 %3, 8
  br i1 %19, label %20, label %22

20:                                               ; preds = %75, %17
  %21 = phi i64 [ 0, %17 ], [ %23, %75 ]
  br label %79

22:                                               ; preds = %17
  %23 = and i64 %18, 4294967288
  %24 = insertelement <4 x i32> undef, i32 %0, i32 0
  %25 = shufflevector <4 x i32> %24, <4 x i32> undef, <4 x i32> zeroinitializer
  %26 = add nsw i64 %23, -8
  %27 = lshr exact i64 %26, 3
  %28 = add nuw nsw i64 %27, 1
  %29 = and i64 %28, 1
  %30 = icmp eq i64 %26, 0
  br i1 %30, label %61, label %31

31:                                               ; preds = %22
  %32 = and i64 %28, 4611686018427387902
  %33 = add i32 %0, 4
  %34 = insertelement <4 x i32> undef, i32 %33, i64 0
  %35 = shufflevector <4 x i32> %34, <4 x i32> undef, <4 x i32> zeroinitializer
  %36 = add i32 %0, 4
  %37 = insertelement <4 x i32> undef, i32 %36, i64 0
  %38 = shufflevector <4 x i32> %37, <4 x i32> undef, <4 x i32> zeroinitializer
  br label %39

39:                                               ; preds = %39, %31
  %40 = phi i64 [ 0, %31 ], [ %57, %39 ]
  %41 = phi <4 x i32> [ <i32 0, i32 1, i32 2, i32 3>, %31 ], [ %58, %39 ]
  %42 = phi i64 [ %32, %31 ], [ %59, %39 ]
  %43 = add <4 x i32> %41, %25
  %44 = add <4 x i32> %35, %41
  %45 = getelementptr i32, i32* %13, i64 %40
  %46 = bitcast i32* %45 to <4 x i32>*
  store <4 x i32> %43, <4 x i32>* %46, align 4, !tbaa !19
  %47 = getelementptr i32, i32* %45, i64 4
  %48 = bitcast i32* %47 to <4 x i32>*
  store <4 x i32> %44, <4 x i32>* %48, align 4, !tbaa !19
  %49 = or i64 %40, 8
  %50 = add <4 x i32> %41, <i32 8, i32 8, i32 8, i32 8>
  %51 = add <4 x i32> %50, %25
  %52 = add <4 x i32> %38, %50
  %53 = getelementptr i32, i32* %13, i64 %49
  %54 = bitcast i32* %53 to <4 x i32>*
  store <4 x i32> %51, <4 x i32>* %54, align 4, !tbaa !19
  %55 = getelementptr i32, i32* %53, i64 4
  %56 = bitcast i32* %55 to <4 x i32>*
  store <4 x i32> %52, <4 x i32>* %56, align 4, !tbaa !19
  %57 = add i64 %40, 16
  %58 = add <4 x i32> %41, <i32 16, i32 16, i32 16, i32 16>
  %59 = add i64 %42, -2
  %60 = icmp eq i64 %59, 0
  br i1 %60, label %61, label %39, !llvm.loop !26

61:                                               ; preds = %39, %22
  %62 = phi i64 [ 0, %22 ], [ %57, %39 ]
  %63 = phi <4 x i32> [ <i32 0, i32 1, i32 2, i32 3>, %22 ], [ %58, %39 ]
  %64 = icmp eq i64 %29, 0
  br i1 %64, label %75, label %65

65:                                               ; preds = %61
  %66 = add <4 x i32> %63, %25
  %67 = add i32 %0, 4
  %68 = insertelement <4 x i32> undef, i32 %67, i64 0
  %69 = shufflevector <4 x i32> %68, <4 x i32> undef, <4 x i32> zeroinitializer
  %70 = add <4 x i32> %69, %63
  %71 = getelementptr i32, i32* %13, i64 %62
  %72 = bitcast i32* %71 to <4 x i32>*
  store <4 x i32> %66, <4 x i32>* %72, align 4, !tbaa !19
  %73 = getelementptr i32, i32* %71, i64 4
  %74 = bitcast i32* %73 to <4 x i32>*
  store <4 x i32> %70, <4 x i32>* %74, align 4, !tbaa !19
  br label %75

75:                                               ; preds = %61, %65
  %76 = icmp eq i64 %23, %18
  br i1 %76, label %77, label %20

77:                                               ; preds = %79, %75, %2
  %78 = bitcast i8* %4 to %struct.Array*
  ret %struct.Array* %78

79:                                               ; preds = %20, %79
  %80 = phi i64 [ %84, %79 ], [ %21, %20 ]
  %81 = trunc i64 %80 to i32
  %82 = add i32 %81, %0
  %83 = getelementptr i32, i32* %13, i64 %80
  store i32 %82, i32* %83, align 4, !tbaa !19
  %84 = add nuw nsw i64 %80, 1
  %85 = icmp eq i64 %84, %18
  br i1 %85, label %77, label %79, !llvm.loop !27
}

; Function Attrs: nofree nounwind sspstrong uwtable
define noalias %struct.Array* @rangeArray3(i32 %0, i32 %1, i32 %2) local_unnamed_addr #2 {
  %4 = sub i32 %1, %0
  %5 = sdiv i32 %4, %2
  %6 = srem i32 %4, %2
  %7 = icmp sgt i32 %6, 0
  %8 = zext i1 %7 to i32
  %9 = add i32 %5, %8
  %10 = tail call noalias dereferenceable_or_null(32) i8* @malloc(i64 32) #6
  %11 = bitcast i8* %10 to i64*
  store i64 0, i64* %11, align 8, !tbaa !15
  %12 = sext i32 %9 to i64
  %13 = getelementptr inbounds i8, i8* %10, i64 16
  %14 = bitcast i8* %13 to i64*
  store i64 %12, i64* %14, align 8, !tbaa !3
  %15 = getelementptr inbounds i8, i8* %10, i64 8
  %16 = bitcast i8* %15 to i64*
  store i64 %12, i64* %16, align 8, !tbaa !9
  %17 = shl nsw i64 %12, 2
  %18 = tail call noalias i8* @malloc(i64 %17) #6
  %19 = bitcast i8* %18 to i32*
  %20 = getelementptr inbounds i8, i8* %10, i64 24
  %21 = bitcast i8* %20 to i8**
  store i8* %18, i8** %21, align 8, !tbaa !10
  %22 = icmp sgt i32 %2, 0
  %23 = icmp sgt i32 %1, %0
  %24 = and i1 %22, %23
  br i1 %24, label %25, label %31

25:                                               ; preds = %3, %25
  %26 = phi i32 [ %29, %25 ], [ %0, %3 ]
  %27 = sext i32 %26 to i64
  %28 = getelementptr i32, i32* %19, i64 %27
  store i32 %26, i32* %28, align 4, !tbaa !19
  %29 = add i32 %26, %2
  %30 = icmp slt i32 %29, %1
  br i1 %30, label %25, label %31

31:                                               ; preds = %25, %3
  %32 = bitcast i8* %10 to %struct.Array*
  ret %struct.Array* %32
}

; Function Attrs: argmemonly nounwind willreturn
declare void @llvm.memmove.p0i8.p0i8.i64(i8* nocapture, i8* nocapture readonly, i64, i1 immarg) #5

; Function Attrs: argmemonly nounwind willreturn
declare void @llvm.memcpy.p0i8.p0i8.i64(i8* noalias nocapture writeonly, i8* noalias nocapture readonly, i64, i1 immarg) #5

attributes #0 = { nounwind sspstrong uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="4" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #1 = { nounwind "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="4" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #2 = { nofree nounwind sspstrong uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="4" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #3 = { nofree norecurse nounwind sspstrong uwtable "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "min-legal-vector-width"="0" "no-infs-fp-math"="false" "no-jump-tables"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="4" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #4 = { nofree nounwind "correctly-rounded-divide-sqrt-fp-math"="false" "disable-tail-calls"="false" "frame-pointer"="none" "less-precise-fpmad"="false" "no-infs-fp-math"="false" "no-nans-fp-math"="false" "no-signed-zeros-fp-math"="false" "no-trapping-math"="true" "stack-protector-buffer-size"="4" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "unsafe-fp-math"="false" "use-soft-float"="false" }
attributes #5 = { argmemonly nounwind willreturn }
attributes #6 = { nounwind }

!llvm.module.flags = !{!0, !1}
!llvm.ident = !{!2}

!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!2 = !{!"clang version 11.1.0"}
!3 = !{!4, !5, i64 16}
!4 = !{!"Array", !5, i64 0, !5, i64 8, !5, i64 16, !8, i64 24}
!5 = !{!"long", !6, i64 0}
!6 = !{!"omnipotent char", !7, i64 0}
!7 = !{!"Simple C/C++ TBAA"}
!8 = !{!"any pointer", !6, i64 0}
!9 = !{!4, !5, i64 8}
!10 = !{!4, !8, i64 24}
!11 = !{!5, !5, i64 0}
!12 = distinct !{!12, !13}
!13 = !{!"llvm.loop.unroll.disable"}
!14 = distinct !{!14, !13}
!15 = !{!4, !5, i64 0}
!16 = distinct !{!16, !13}
!17 = distinct !{!17, !13}
!18 = distinct !{!18, !13}
!19 = !{!20, !20, i64 0}
!20 = !{!"int", !6, i64 0}
!21 = distinct !{!21, !22}
!22 = !{!"llvm.loop.isvectorized", i32 1}
!23 = distinct !{!23, !13}
!24 = distinct !{!24, !25, !22}
!25 = !{!"llvm.loop.unroll.runtime.disable"}
!26 = distinct !{!26, !22}
!27 = distinct !{!27, !25, !22}
